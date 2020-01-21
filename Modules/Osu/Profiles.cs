using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CyanBot.Modules.OsuUtils {
    public class Profile {
        public int Id { get; set; }
        public long qq_id { get; set; }
        public string user_id { get; set; }
        public string username { get; set; }
        public string count300 { get; set; }
        public string count100 { get; set; }
        public string count50 { get; set; }
        public string pp_raw { get; set; }
        public string pp_rank { get; set; }
        public string level { get; set; }
    }
    public static class Profiles {
        static HttpClient client = new HttpClient ();
        static Dictionary<long, DateTime> last_update = new Dictionary<long, DateTime> ();
        static int minute_calls = 0;
        async static Task<JToken> Call (string path, params (string, string) [] parameters) {
            while (minute_calls > 60) Thread.Sleep (1000);
            var p = new List < (string, string) > (parameters);
            p.Add (("k", Program.Globals["osu_token"]));
            StringBuilder url = new StringBuilder ("https://osu.ppy.sh/api/");
            url.Append (path.Trim ('/'));
            url.Append ('?');
            url.AppendJoin (
                '&', p.ConvertAll<string> (
                    (o) => string.Concat (o.Item1, '=', o.Item2)
                ));
            var t = Task.Run (() => {
                minute_calls++;
                Thread.Sleep (60000);
                minute_calls--;
            });
            return JToken.Parse (
                await client.GetStringAsync (url.ToString ())
            );
        }
        static LiteDB.LiteDatabase db = new LiteDB.LiteDatabase ("Filename=osu.db;mode=Exclusive");
        static LiteDB.LiteCollection<Profile> col {
            get { return db.GetCollection<Profile> ("osu_profiles"); }
        }
        public static Profile Fetch (string username) {
            JToken result = Call (
                "get_user",
                ("u", username),
                ("m", "0"),
                ("type", "string")
            ).Result[0];
            return result.ToObject<Profile> ();
        }
        public static Profile Get (long qq_id) {
            var p = col.FindOne (x => x.qq_id == qq_id);
            if (p != null && (!last_update.ContainsKey (qq_id) || DateTime.Now - last_update[qq_id] > TimeSpan.FromMinutes (1))) {
                p = Fetch (p.username);
                col.Update (p);
                last_update[qq_id] = DateTime.Now;
            }
            return p;
        }
        public static Profile Get (string username) {
            var p = col.FindOne (x => x.username == username);
            if (p != null && (!last_update.ContainsKey (p.qq_id) || DateTime.Now - last_update[p.qq_id] > TimeSpan.FromMinutes (1))) {
                p = Fetch (p.username);
                col.Update (p);
                last_update[p.qq_id] = DateTime.Now;
            }
            return p;
        }
        public static IEnumerable<Profile> GetAll () {
            var all = col.FindAll ();
            foreach (var p in all)
                Get (p.qq_id); // update outdated profiles
            return col.FindAll ();
        }
        public static Profile Bind (long qq_id, string username) {
            var profile = Fetch (username);
            if (profile != null) {
                profile.qq_id = qq_id;
                var orig = col.FindOne (x => x.qq_id == qq_id);
                if (orig != null) {
                    profile.Id = orig.Id;
                    col.Update (profile);
                } else
                    col.Insert (profile);
            }
            return profile;
        }
    }
}