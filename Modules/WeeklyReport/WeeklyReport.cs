using System.Linq;
using System.Threading.Tasks;
using cqhttp.Cyan.Enums;
using cqhttp.Cyan.Events.CQEvents.Base;
using cqhttp.Cyan.Messages;
using CyanBot.Modules.WeeklyReportUtils;

namespace CyanBot.Modules {
    class WeeklyReport : Module {
        [OnCommand ("get_wr")]
        public static Message GetWR (string[] parameters, MessageEvent e) {
            if (e.GetEndpoint ().Item1 != MessageType.private_) {
                return new Message ("private chat only");
            }
            Task.Run (async () => {
                if (await Creds.ensureLoggedIn (e.GetEndpoint ().Item2)) {
                    string ret = "目前有: ";
                    WRScraper.submittedUsers.ForEach ((s) => ret += s + ',');
                    ret += "已经提交了周报\n你要看谁的？";
                    await Program.client.SendTextAsync (
                        e.GetEndpoint (), ret
                    );
                }
            }).ContinueWith ((t) => {
                var d = new cqhttp.Cyan.Utils.Dialogue ();
                d["BEGIN"] = async (c, m) => {
                    await c.SendTextAsync (
                        e.GetEndpoint (),
                        WRScraper.getWRFor (
                            Creds.loggedIn[e.GetEndpoint ().Item2], m.GetRaw ()).Result
                    );
                    return "DONE";
                };
                cqhttp.Cyan.Utils.DialoguePool.Join (e.GetEndpoint (), d);
            });
            return new Message ();
        }

        [OnCommand ("init")]
        public static Message Init (string[] parameters, MessageEvent e) {
            if (e.GetEndpoint ().Item1 != MessageType.private_) {
                return new Message ("private chat only");
            }
            Program.client.SendTextAsync (
                e.GetEndpoint (),
                "请输入登陆所使用的昵称"
            ).ContinueWith ((t) => {
                var d = new cqhttp.Cyan.Utils.Dialogue ();
                string username = "", password = "";
                d["BEGIN"] = async (c, m) => {
                    username = m.GetRaw ();
                    await c.SendTextAsync (e.GetEndpoint (), "请输入token");
                    return "SET_TOKEN";
                };
                d["SET_TOKEN"] = async (c, m) => {
                    password = m.GetRaw ();
                    Creds.PersistData (e.GetEndpoint ().Item2, username, password);
                    await c.SendTextAsync (e.GetEndpoint (), $"设置完成,用户名:{username},token:{password}");
                    return "DONE";
                };
                cqhttp.Cyan.Utils.DialoguePool.Join (e.GetEndpoint (), d);
            });
            return new Message ();
        }

        [OnCommand ("submit")]
        public static Message Submit (string[] parameters, MessageEvent e) {
            if (System.DateTime.UtcNow.AddHours (8).DayOfWeek != System.DayOfWeek.Sunday)
                return new Message ("only sunday");
            if (Creds.ensureLoggedIn (e.GetEndpoint ().Item2).Result == false)
                return new Message ("请重新进行/init");
            var d = new cqhttp.Cyan.Utils.Dialogue ();
            d["BEGIN"] = async (c, m) => {
                await WRScraper.submitWR (
                    Creds.loggedIn[e.GetEndpoint ().Item2],
                    m.GetRaw ()
                );
                await c.SendTextAsync (e.GetEndpoint (), "已提交");
                await WRScraper.updateIndex ();
                return "DONE";
            };
            cqhttp.Cyan.Utils.DialoguePool.Join (e.GetEndpoint (), d);
            return new Message ("请发送周报的内容:");
        }

        [OnCommand ("status")]
        public async static Task<Message> Status (string[] parameters, MessageEvent e) {
            await WRScraper.updateIndex ();
            return new Message ($"{WRScraper.submittedUsers.Count}人已提交周报");
        }
        [OnCommand ("remind")]
        public async static Task<Message> Remind (string[] parameters, MessageEvent e) {
            await WRScraper.updateIndex ();
            var unsubmitted = (
                from user in WRScraper.users.Keys
                where !WRScraper.submittedUsers.Contains (user)
                select user
            ).ToList ();
            if (unsubmitted.Count > 0) {
                string toSend = $"仍然有{unsubmitted.Count}名铁憨憨没有交周报。他们分别是:\n";
                unsubmitted.ForEach ((s) => toSend += s + ',');
                toSend = toSend.TrimEnd (',');
                toSend += "\n清退警告⚠️";
                return new Message (toSend);
            }
            return new Message ("终于有一次全员交齐了");
        }
    }
}