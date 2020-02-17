using cqhttp.Cyan.Messages;
using Newtonsoft.Json.Linq;

namespace CyanBot.Modules.CTFdUtils {
    static class Get {
        static cqhttp.Cyan.Utils.Logger logger =
            cqhttp.Cyan.Utils.Logger.GetLogger ("CTFd");
        public static Message Message (Event e) {
            switch (e.type) {
            case "challenge":
                return FromChallenge (JToken.Parse (e.data));
            case "hint":
                return null;
            case "ping":
                logger.Debug ("ping from CTFd");
                return null;
            default:
                logger.Warn ("unknown event type: " + e.type);
                return null;
            }
        }
        static Message FromChallenge(JToken data) {
            switch (data["type"].ToObject<string> ()) {
                case "challenge_solved":
                return new Message ($"{data["user"]["name"]}做出了{data["challenge"]}");
                case "challenge_created":
                break;
                case "challenge_updated":
                break;
            }
            return null;
        }
    }
}