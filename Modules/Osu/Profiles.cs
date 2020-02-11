using System;
using System.Collections.Generic;
using System.Linq;
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
        async static Task<JToken> Call (string path, params (string, string)[] parameters) {
            var wait = Task.Run (() => { while (minute_calls > 60) Thread.Sleep (1000); });
            var p = new List<(string, string)> (parameters);
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
            await wait;
            return JToken.Parse (
                await client.GetStringAsync (url.ToString ())
            );
        }
        static LiteDB.LiteDatabase db = new LiteDB.LiteDatabase (
            "Filename=osu.db;mode=Exclusive"
        );
        static LiteDB.LiteCollection<Profile> col {
            get { return db.GetCollection<Profile> ("osu_profiles"); }
        }
        static Dictionary<string, string> mode_code = new Dictionary<string, string> {
            ["std"] = "0",
            ["taiko"] = "1",
            ["catch"] = "2",
            ["mania"] = "3"
        };
        public static Profile Fetch (string username) {
            string mode = "std";
            if (username.Contains ("@")) {
                var d = username.Split ('@');
                (username, mode) = (d[0], d[1]);
            }
            JToken result = Call (
                "get_user",
                ("u", username),
                ("m", mode_code[mode]),
                ("type", "string")
            ).Result[0];
            result["username"] = string.Join (
                '@', result["username"].ToObject<string> (), mode
            );
            return result.ToObject<Profile> ();
        }
        public static Profile Get (long qq_id) {
            var p = col.FindOne (x => x.qq_id == qq_id);
            if (p != null && (!last_update.ContainsKey (qq_id) || (DateTime.Now - last_update[qq_id] > TimeSpan.FromMinutes (1)))) {
                var new_profile = Fetch (p.username);
                new_profile.Id = p.Id;
                new_profile.qq_id = p.qq_id;
                col.Update (new_profile);
                p = new_profile;
                last_update[qq_id] = DateTime.Now;
            }
            return p;
        }
        public static Profile Get (string username) {
            var p = col.FindOne (x => x.username.Contains (username));
            if (p == null)
                return Fetch (username);
            return Get (p.qq_id);
        }
        public static IEnumerable<Profile> GetAll () {
            var all = col.FindAll ();
            return all.Select ((x) => {
                var new_profile = Get (x.username);
                new_profile.qq_id = x.qq_id;
                return new_profile;
            });
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