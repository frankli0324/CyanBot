using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using cqhttp.Cyan.Enums;
using cqhttp.Cyan.Events.CQEvents.Base;
using cqhttp.Cyan.Messages;
using Newtonsoft.Json.Linq;

namespace CyanBot.Modules {
    public class Wuhan : Module {
        static HashSet < (MessageType, long) > endpoints =
            new HashSet < (MessageType, long) > ();
        static Dictionary<string, JToken> info =
            new Dictionary<string, JToken> ();
        static JToken obj;
        static string latest_news;
        static HttpClient client = new HttpClient ();
        static async Task SendNewInfo (JToken new_info, JToken old_info) {
            StringBuilder msg = new StringBuilder ();
            string province = new_info["name"].ToString ();
            Action<string, string> d = (key, desc) => {
                int new_cnt = new_info[key].ToObject<int> ();
                int old_cnt = old_info[key].ToObject<int> ();
                if (new_cnt > old_cnt)
                    msg.AppendLine (province + "新增" + desc + (new_cnt - old_cnt) + "例");
            };
            d ("value", "确诊");
            d ("cureNum", "痊愈");
            d ("susNum", "疑似患者");

            if (msg.Length > 0)
                await Task.WhenAll (
                    endpoints.Select (x =>
                        Program.client.SendTextAsync (
                            x, msg.ToString ().TrimEnd ()
                        )
                    ).ToArray ()
                );
        }
        static Wuhan () {
            Task.Run (async () => {
                while (true) {
                    try {
                        string page = await client.GetStringAsync (
                            "https://interface.sina.cn/news/wap/fymap2020_data.d.json"
                        );
                        StringBuilder msg = new StringBuilder ();
                        obj = JToken.Parse (page);
                        foreach (var prov in obj["data"]["list"]) {
                            JToken old_info;
                            if (info.TryGetValue (prov["name"].ToString (), out old_info)) {
                                if (prov != old_info)
                                    await SendNewInfo (prov, old_info);
                            }
                            info[prov["name"].ToString ()] = prov;
                        }
                    } catch { }
                    Thread.Sleep (10000);
                }
            });
            Task.Run (async () => {
                while (true) {
                    // try {
                    string page = await client.GetStringAsync (
                        "https://interface.sina.cn/wap_api/wap_std_subject_components_list.d.json?url=https://news.sina.cn/zt_d/yiqing0121&page=2"
                    );
                    obj = Newtonsoft.Json.Linq.JToken.Parse (page);
                    string news = obj["result"]["data"]["components"][1]["data"][0]["title"].ToString ();
                    string url = obj["result"]["data"]["components"][1]["data"][0]["url"].ToString ();
                    if (news != latest_news) {
                        latest_news = news;
                        await Task.WhenAll (
                            endpoints.Select (x =>
                                Program.client.SendMessageAsync (
                                    x, new Message (string.Join ('\n', news, url))
                                )
                            ).ToArray ()
                        );
                    }
                    // } catch { }
                    Thread.Sleep (10000);
                }
            });
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

        [OnCommand ("fetch_latest")]
        public Message Manual (string[] parameters, MessageEvent e) =>
            new Message (latest_news);

        [OnCommand ("list_current_endpoints")]
        public Message List (string[] parameters, MessageEvent e) {
            string m = "";
            foreach (var i in endpoints)
                m += i.ToString () + "\n";
            return new Message (m.TrimEnd ('\n'));
        }

        [OnCommand ("dstat")]
        public Message Statistics (string[] parameters, MessageEvent e) {
            Func<string, string, string, string, string, Message> d = (p, a, d, c, s) =>
                new Message ((
                    p + "共计:\n" +
                    (a != "0" ? "确诊" + a + "例\n" : "") +
                    (d != "0" ? "死亡" + d + "例\n" : "") +
                    (c != "0" ? "痊愈" + c + "例\n" : "") +
                    (s != "0" ? "疑似" + s + "例\n" : "")
                ).TrimEnd ('\n'));

            if (parameters.Length == 0) {
                return d (
                    "国内",
                    obj["data"]["gntotal"].ToString (),
                    obj["data"]["deathtotal"].ToString (),
                    obj["data"]["curetotal"].ToString (),
                    obj["data"]["sustotal"].ToString ()
                );
            }
            string prov = parameters[0];
            if (info.ContainsKey (prov) == false)
                return new Message ();
            var prov_obj = info[prov];

            if (parameters.Length >= 2) {
                foreach (var o in prov_obj["city"])
                    if (o["name"].ToString () == parameters[1]) {
                        return d (
                            o["name"].ToString (),
                            o["conNum"].ToString (),
                            o["deathNum"].ToString (),
                            o["cureNum"].ToString (),
                            o["susNum"].ToString ()
                        );
                    }
            }
            return d (
                prov_obj["name"].ToString (),
                prov_obj["value"].ToString (),
                prov_obj["deathNum"].ToString (),
                prov_obj["cureNum"].ToString (),
                prov_obj["susNum"].ToString ()
            );
        }

        [OnCommand ("add_groups")]
        public Message AddGroups (string[] parameters, MessageEvent e) {
            foreach (var i in parameters)
                endpoints.Add ((MessageType.group_, long.Parse (i)));
            return new Message ();
        }
    }
}