using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using cqhttp.Cyan.Events.CQEvents.Base;
using cqhttp.Cyan.Messages;
using cqhttp.Cyan.Messages.CQElements;
using CyanBot.Modules.OsuUtils;
using Newtonsoft.Json.Linq;

namespace CyanBot.Modules {
    public class Osu : Module {
        static HttpClient client = new HttpClient ();

        [OnCommand ("bind")]
        public Message BindUser (string[] parameters, MessageEvent e) {
            // if (e.GetEndpoint () != (cqhttp.Cyan.Enums.MessageType.group_, 915383167))
            //     return new Message ("只能在XDOSU群里用");
            if (parameters.Length != 1) return new Message ("specify username");
            Profiles.Bind (e.sender.user_id, parameters[0]);
            return GetPlayerPP (new string[] { }, e);
        }

        [OnCommand ("get_pp")]
        public Message GetPlayerPP (string[] parameters, MessageEvent e) {
            // if (e.GetEndpoint () != (cqhttp.Cyan.Enums.MessageType.group_, 915383167))
            //     return new Message ("只能在XDOSU群里用");
            var profile = parameters.Length > 0 ?
                Profiles.Get (parameters[0]) :
                Profiles.Get (e.sender.user_id);
            return new Message (profile != null ? profile.pp_raw : "not binded yet");
        }

        [OnCommand ("get_profile")]
        public Message GetPlayerProfile (string[] parameters, MessageEvent e) {
            var profile = parameters.Length > 0 ?
                Profiles.Get (parameters[0]) :
                Profiles.Get (e.sender.user_id);
            return new Message (string.Join ('\n', new string[] {
                "pp:" + profile.pp_raw,
                "rank:" + profile.pp_rank,
                "level:" + profile.level
            }));
        }

        [OnCommand ("ranklist")]
        public Message GetRanklist (string[] parameters, MessageEvent e) {
            // if (e.GetEndpoint () != (cqhttp.Cyan.Enums.MessageType.group_, 915383167))
            //     return new Message ("只能在XDOSU群里用");
            var res = Profiles.GetAll ().OrderBy (
                    x => int.Parse (x.pp_rank)
                ).Select (
                    x => string.Join (": ", x.username, x.pp_raw)
                ).ToArray ();
            return new Message (string.Join ('\n', res));
        }

        [OnCommand ("get_beatmap")]
        public Message GetBeatmap (string[] parameters, MessageEvent e) {
            var result = JToken.Parse (client.GetStringAsync (
                "https://osusearch.com/query/?" +
                string.Join ('&', new List<(string, string)> (new (string, string)[] {
                    ("title", parameters[0]),
                    ("modes", "Standard"),
                    ("statuses", "Ranked")
                }).ConvertAll<string> (
                    (o) => string.Concat (o.Item1, '=', o.Item2)
                ))).Result)["beatmaps"][0];
            return new Message (new ElementShare (
                "https://osu.ppy.sh/b/" + result["beatmap_id"],
                result["title"] + " by " + result["artist"],
                $"Difficulty: {result["difficulty"]}\nMapper: {result["mapper"]}",
                $"https://assets.ppy.sh/beatmaps/{result["beatmapset_id"]}/covers/card.jpg?1579536000000"
            ));
        }
    }
}