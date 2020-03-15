using cqhttp.Cyan.Messages;
using Newtonsoft.Json.Linq;

namespace CyanBot.Modules.CTFdUtils {
    static class Get {
        static cqhttp.Cyan.Utils.Logger logger =
            cqhttp.Cyan.Utils.Logger.GetLogger ("CTFd");
        public static Message Message (Event e) {
            switch (e.type) {
            case "challenge":
                return FromChallenge (e.data);
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
        static Message FromChallenge (string data) {
            var json = JToken.Parse (data);
            switch (json["type"].ToObject<string> ()) {
            case "challenge_solved":
                return new Message ($"{json["user"]["name"]}做出了{json["challenge"]}");
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