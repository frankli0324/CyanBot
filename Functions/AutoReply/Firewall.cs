using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using cqhttp.Cyan.Enums;

namespace CyanBot.Functions.AutoReplyUtils {
    class Firewall {
        class TimeoutStringSet : HashSet<string> {
            public void TimeoutAdd (string s) {
                if (!this.Contains (s))
                    this.Add (s);
                Task.Run (() => {
                    Thread.Sleep (3000);
                    this.Remove (s);
                });
            }

        }

        static Dictionary < (MessageType, long), TimeoutStringSet > firewall =
            new Dictionary < (MessageType, long), TimeoutStringSet > ();
        public static bool waf ((MessageType, long) endpoint, string message) {
            if (!firewall.ContainsKey (endpoint))
                firewall[endpoint] = new TimeoutStringSet ();
            if (firewall[endpoint].Contains (message))
                return false;
            else {
                firewall[endpoint].TimeoutAdd (message);
                return true;
            }
        }
    }
}