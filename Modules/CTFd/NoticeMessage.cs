using System.Threading.Tasks;
using cqhttp.Cyan.Messages;
using Newtonsoft.Json.Linq;

namespace CyanBot.Modules.CTFdUtils {
    static class Get {
        static cqhttp.Cyan.Utils.Logger logger =
            cqhttp.Cyan.Utils.Logger.GetLogger ("CTFd");
        public async static Task<Message> Message (Event e, CTFdClient client = null) {
            switch (e.type) {
            case "challenge":
                return await FromChallenge (e.data, client);
            case "hint":
                return null;
            case "ping":
                logger.Debug ("ping from CTFd");
                return null;
            case "notification":
                logger.Debug ("published notification");
                return FromNotice (e.data);
            default:
                logger.Warn ("unknown event type: " + e.type);
                return null;
            }
        }
        async static Task<Message> FromChallenge (string data, CTFdClient client = null) {
            var json = JToken.Parse (data);
            switch (json["type"].ToObject<string> ()) {
            case "challenge_solved":
                var t = JToken.Parse (await (await client.GetAsync (
                    $"{client.host.TrimEnd ('/')}/api/v1/challenges/" + json["challenge"]
                )).Content.ReadAsStringAsync());
                return new Message ($"{json["user"]["name"]}做出了{t["data"]["name"]}");
            case "challenge_created":
                break;
            case "challenge_updated":
                break;
            }
            return null;
        }
        static Message FromNotice (string data) {
            return null;
        }
    }
}