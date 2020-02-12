using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using cqhttp.Cyan.Enums;

namespace CyanBot.Modules.AutoReplyUtils {
    static class TimeoutExtension {
        public static void TimeoutAdd (this HashSet<string> set, string s) {
            if (!set.Contains (s))
                set.Add (s);
            Task.Run (() => {
                Thread.Sleep (10000);
                set.Remove (s);
            });
        }
    }
    class Firewall {
        static Dictionary < (MessageType, long), HashSet<string>> firewall =
            new Dictionary < (MessageType, long), HashSet<string>> ();
        public static bool waf ((MessageType, long) endpoint, string message) {
            if (!firewall.ContainsKey (endpoint))
                firewall[endpoint] = new HashSet<string> ();
            if (firewall[endpoint].Contains (message))
                return false;
            else {
                firewall[endpoint].TimeoutAdd (message);
                return true;
            }
        }
    }
}