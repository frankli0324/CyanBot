using System.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using cqhttp.Cyan.Enums;
using cqhttp.Cyan.Events.CQEvents.Base;
using cqhttp.Cyan.Messages;
using Newtonsoft.Json.Linq;
using CyanBot.Modules.WuhanUtils;

namespace CyanBot.Modules {
    public class Wuhan : Module {
        static HashSet<(MessageType, long)> endpoints =
            new HashSet<(MessageType, long)> ();
        static HttpClient client = new HttpClient ();
        static Dictionary<string, Area> data = new Dictionary<string, Area> ();
        static HashSet<string> old_news = new HashSet<string> ();
        static System.Threading.Timer t = new System.Threading.Timer (async (o) => {
            {
                string page = await client.GetStringAsync (
                    "https://c.m.163.com/ug/api/wuhan/app/data/list-total"
                );
                var obj = JToken.Parse (page);
                foreach (var a in obj["data"]["areaTree"]) {
                    string country = a["name"].ToObject<string> ();
                    if (country == "中国" && data.ContainsKey ("中国")) {
                        var msg = Diff (data["中国"], new Area (a));
                        if (msg != null)
                            foreach (var endpoint in endpoints)
                                await Program.client.SendMessageAsync (
                                    endpoint, msg
                                );
                    }
                    data[country] = new Area (a);
                }
            }
            {
                string page = await client.GetStringAsync (
                    "https://interface.sina.cn/wap_api/wap_std_subject_components_list.d.json?url=https://news.sina.cn/zt_d/yiqing0121&page=2"
                );
                var obj = JToken.Parse (page);
                obj = obj["result"]["data"]["components"][2]["data"][0];
                string url = obj["url"].ToObject<string> ();
                string media = obj["media"].ToObject<string> ();
                string title = obj["title"].ToObject<string> ();
                if (!old_news.Contains (title)) {
                    old_news.Add (title);
                    string msg = string.Join ('\n', title, url, "from:" + media);
                    foreach (var endpoint in endpoints)
                        await Program.client.SendTextAsync (
                            endpoint, msg
                        );
                }
            }
        }, null, 0, 10000);
        static Message Diff (Area o, Area n) {
            StringBuilder msg = new StringBuilder ();
            Action<string, string, string> d = (area, key, desc) => {
                int new_cnt = o[area].total[key].ToObject<int> ();
                int old_cnt = n[area].total[key].ToObject<int> ();
                if (new_cnt > old_cnt)
                    msg.AppendLine (area + "新增" + desc + (new_cnt - old_cnt) + "例");
            };
            foreach (var sub in o) {
                d (sub.name, "confirm", "确诊");
                d (sub.name, "suspect", "疑似");
                d (sub.name, "heal", "治愈");
                // d (sub.name, "dead", "死亡");
            }
            if (msg.Length > 0)
                return new Message (msg.ToString ().TrimEnd ());
            else return null;
        }

        [OnCommand ("alert_disease")]
        public Message StartAlarm (string[] parameters, MessageEvent e) {
            endpoints.Add (e.GetEndpoint ());
            System.Console.WriteLine ("added to set " + e.GetEndpoint ());
            return new Message ();
        }

        [OnCommand ("stop_alert")]
        public Message EndAlarm (string[] parameters, MessageEvent e) {
            endpoints.Remove (e.GetEndpoint ());
            System.Console.WriteLine ("removed from set " + e.GetEndpoint ());
            return new Message ();
        }

        [OnCommand ("list_current_endpoints")]
        public Message List (string[] parameters, MessageEvent e) {
            string m = "";
            foreach (var i in endpoints)
                m += i.ToString () + "\n";
            return new Message (m.TrimEnd ('\n'));
        }

        [OnCommand ("dstat")]
        public Message Wrap1 (string[] parameters, MessageEvent e) => Statistics (parameters, e);
        public Message Statistics (string[] parameters, MessageEvent e, bool total = true) {
            var root = data["中国"];
            foreach (var i in parameters)
                if (root.ContainsKey (i))
                    root = root[i];
            var stat = total?root.total:root.today;
            return new Message ((
                root.name + (total ? "共计:\n" : "今日新增\n") +
                (stat.confirmed () != 0 ? "确诊" + stat.confirmed () + "例\n" : "") +
                (stat.dead () != 0 ? "死亡" + stat.dead () + "例\n" : "") +
                (stat.healed () != 0 ? "痊愈" + stat.healed () + "例\n" : "") +
                (stat.suspected () != 0 ? "疑似" + stat.suspected () + "例\n" : "")
            ).TrimEnd ('\n'));
        }

        [OnCommand ("dstat_country")]
        public Message Wrap2 (string[] parameters, MessageEvent e) => OtherCountry (parameters, e);
        public Message OtherCountry (string[] parameters, MessageEvent e, bool total = true) {
            if (parameters.Length == 0 || !data.ContainsKey (parameters[0]))
                return new Message ();
            var root = data[parameters[0]];
            var stat = total?root.total:root.today;
            return new Message ((
                root.name + (total ? "共计:\n" : "今日新增\n") +
                (stat.confirmed () != 0 ? "确诊" + stat.confirmed () + "例\n" : "") +
                (stat.dead () != 0 ? "死亡" + stat.dead () + "例\n" : "") +
                (stat.healed () != 0 ? "痊愈" + stat.healed () + "例\n" : "") +
                (stat.suspected () != 0 ? "疑似" + stat.suspected () + "例\n" : "")
            ).TrimEnd ('\n'));
        }

        [OnCommand ("today")]
        public Message Today (string[] parameters, MessageEvent e) {
            if (parameters.Length == 0)
                return new Message ("example: \"/today dstat 陕西 西安\" or \"/today dstat_country 美国\"");
            if (parameters[0] == "dstat")
                return Statistics (parameters.Skip (1).ToArray (), e, total: false);
            else if (parameters[0] == "dstat_country")
                return OtherCountry (parameters.Skip (1).ToArray (), e, total: false);
            return new Message ();
        }

        [OnCommand ("add_groups")]
        public Message AddGroups (string[] parameters, MessageEvent e) {
            foreach (var i in parameters)
                endpoints.Add ((MessageType.group_, long.Parse (i)));
            return new Message ();
        }
    }
}