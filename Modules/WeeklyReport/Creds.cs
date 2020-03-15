using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CyanBot.Modules.WeeklyReportUtils {
    class Creds {
        static JObject credentials = new JObject ();
        static Dictionary < long, (string, string) > cache = new Dictionary < long, (string, string) > ();
        public static Dictionary<long, HttpClient> loggedIn = new Dictionary<long, HttpClient> ();
        public static void LoadData () {
            if (File.Exists ("creds.json") == false) File.Create ("creds.json");
            credentials = JObject.Parse (File.ReadAllText ("creds.json"));
        }
        public static void PersistData (long user_id, string username, string password) {
            var entry = new JObject ();
            entry["username"] = username;
            entry["password"] = password;
            credentials[user_id.ToString ()] = entry;
            cache[user_id] = (username, password);
            File.WriteAllText ("creds.json", credentials.ToString (Formatting.None));
        }
        public static (string, string) getCredentials (long user_id) {
            if (cache.ContainsKey (user_id))
                return cache[user_id];
            if (credentials.ContainsKey (user_id.ToString ())) {
                var temp = credentials[user_id.ToString ()];
                cache[user_id] = (temp["username"].ToObject<string> (), temp["password"].ToObject<string> ());
                return cache[user_id];
            } else throw new System.UnauthorizedAccessException ();
        }
        public static async Task<bool> ensureLoggedIn (long user_id) {
            if (loggedIn.ContainsKey (user_id) == false) {
                (string, string) cred = ("", "");
                loggedIn[user_id] = new HttpClient ();
                try {
                    cred = getCredentials (user_id);
                } catch (System.UnauthorizedAccessException) {
                    return false;
                }
                var resp = await loggedIn[user_id].PostAsync ("http://wr.xdsec.club/login", new FormUrlEncodedContent (
                    new List<KeyValuePair<string, string>> {
                        new KeyValuePair<string, string> ("username", cred.Item1),
                        new KeyValuePair<string, string> ("passwd", cred.Item2),
                        new KeyValuePair<string, string> ("login", "Login")
                    }
                ));
                string content = await resp.Content.ReadAsStringAsync ();
                if (content.Contains ("wrong!")) {
                    loggedIn[user_id].Dispose ();
                    loggedIn.Remove (user_id);
                    return false;
                }
                cqhttp.Cyan.Utils.Logger.GetLogger ("wr").Info ($"{user_id} loggid in");
                return true;
            }
            return true;
        }
    }
}