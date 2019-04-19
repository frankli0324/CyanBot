using System;
using System.Collections.Generic;
using cqhttp.Cyan.Messages;
using Newtonsoft.Json.Linq;

namespace CyanBot.Functions {
    public static class Hitokoto {
        static List<string> types = new List<string> {
            "Anime",
            "Comic",
            "Game",
            "Novel",
            "Original",
            "Internet",
            "Other"
        };
        public static Message GetHitokoto (List<string> p) {
            JObject result = new JObject ();
            string url = "https://v1.hitokoto.cn/?encode=json&charset=utf-8";
            if (p.Count > 1) {
                string helpMsg = "Usage: /{hitokoto/一言} [parameter]\n其中:parameter可为空或 ";
                foreach (var i in types)
                    helpMsg += i + ',';
                if (p[1] == "help") return new Message (helpMsg.TrimEnd (',') + " 其中之一");
                else if (types.Contains (p[1]))
                    url += $"&c={Convert.ToChar('a' + types.IndexOf(p[1]))}";
            }
            try {
                using (var http = new System.Net.Http.HttpClient ()) {
                    result = JObject.Parse (http.GetStringAsync (url).Result);
                }
            } catch { return new Message ("网络错误"); }
            return new Message ($"{result["hitokoto"].ToString()}\n--{result["from"].ToString()}");
        }
        public static void Register () {
            FunctionPool.onCommand.Add ("hitokoto", (p) => GetHitokoto (p));
            FunctionPool.onCommand.Add ("一言", (p) => GetHitokoto (p));
        }
    }
}