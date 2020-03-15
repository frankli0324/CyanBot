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
                d["BEGIN"] = (c, m) => {
                    var s = c.SendTextAsync (
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
                d["BEGIN"] = (c, m) => {
                    username = m.GetRaw ();
                    var s = c.SendTextAsync (e.GetEndpoint (), "请输入token");
                    return "SET_TOKEN";
                };
                d["SET_TOKEN"] = (c, m) => {
                    password = m.GetRaw ();
                    Creds.PersistData (e.GetEndpoint ().Item2, username, password);
                    var s = c.SendTextAsync (e.GetEndpoint (), $"设置完成,用户名:{username},token:{password}");
                    return "DONE";
                };
                cqhttp.Cyan.Utils.DialoguePool.Join (e.GetEndpoint (), d);
            });
            return new Message ();
        }

        [OnCommand ("submit")]
        public static Message Submit (string[] parameters, MessageEvent e) {
            if (System.DateTime.UtcNow.AddHours (8).DayOfWeek != System.DayOfWeek.Sunday) {
                var s = Program.client.SendTextAsync (
                    e.GetEndpoint (),
                    "only sunday"
                );
                return new Message ();
            }
            if (Creds.ensureLoggedIn (e.GetEndpoint ().Item2).Result == false) {
                var s = Program.client.SendTextAsync (
                    e.GetEndpoint (),
                    "请重新进行/init"
                );
                return new Message ();
            }
            Program.client.SendTextAsync (
                e.GetEndpoint (),
                "请发送周报的内容:"
            ).ContinueWith ((t) => {
                var d = new cqhttp.Cyan.Utils.Dialogue ();
                d["BEGIN"] = (c, m) => {
                    WRScraper.submitWR (
                        Creds.loggedIn[e.GetEndpoint ().Item2],
                        m.GetRaw ()
                    ).Wait ();
                    var s = c.SendTextAsync (e.GetEndpoint (), "已提交");
                    var sup = WRScraper.updateIndex ();
                    return "DONE";
                };
                cqhttp.Cyan.Utils.DialoguePool.Join (e.GetEndpoint (), d);
            });
            return new Message ();
        }

        [OnCommand ("status")]
        public static Message Status (string[] parameters, MessageEvent e) {
            Task.Run (WRScraper.updateIndex).ContinueWith (async (t) => {
                await Program.client.SendTextAsync (
                    e.GetEndpoint (),
                    $"{WRScraper.submittedUsers.Count}人已提交周报"
                );
            });
            return new Message ();
        }
        [OnCommand ("remind")]
        public static Message Remind (string[] parameters, MessageEvent e) {
            WRScraper.updateIndex ().ContinueWith (async (t) => {
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
                    await Program.client.SendTextAsync (e.GetEndpoint (), toSend);
                }
            });
            return new Message ();
        }
    }
}