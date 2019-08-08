using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using cqhttp.Cyan.Enums;
using cqhttp.Cyan.Instance;
using cqhttp.Cyan.Messages;
using cqhttp.Cyan.Messages.CQElements;
using CyanBot;

namespace CyanBot.Functions {
    public class Time {
        static string[] pics = {
            "https://i.loli.net/2019/04/20/5cba0323b4cdb.png", //12
            "https://i.loli.net/2019/04/20/5cba0332dd371.png", //1
            "https://i.loli.net/2019/04/20/5cba03160903c.png", //2
            "https://i.loli.net/2019/04/20/5cba0319b7415.png", //3
            "https://i.loli.net/2019/04/20/5cba0318dbe4f.png", //4
            "https://i.loli.net/2019/04/20/5cba031e08bfd.png", //5
            "https://i.loli.net/2019/04/20/5cba031ab6ec1.png", //6
            "https://i.loli.net/2019/04/20/5cba031ea09de.png", //7
            "https://i.loli.net/2019/04/20/5cba031995065.png", //8
            "https://i.loli.net/2019/04/20/5cba031bc35e9.png", //9
            "https://i.loli.net/2019/04/20/5cba03172f768.png", //10
            "https://i.loli.net/2019/04/20/5cba03198cab3.png" //11
        };
        static HashSet < (CQApiClient, (MessageType, long)) > alarmList =
            new HashSet < (CQApiClient, (MessageType, long)) > ();
        static bool isStarted = false;
        static System.Timers.Timer t = new System.Timers.Timer (3600000);
        public static void LoadModule () {
            FunctionPool.onCommand.Add ("start_alarm", (p) => {
                if (isStarted == false) {
                    Task.Run (() => {
                        var next_hour = DateTime.UtcNow.AddHours (1);
                        next_hour = next_hour.AddMinutes (-next_hour.Minute);
                        next_hour = next_hour.AddSeconds (-next_hour.Second);
                        Thread.Sleep (next_hour - DateTime.UtcNow);
                        cqhttp.Cyan.Logger.Info ("started timer");
                        t.Elapsed += (o, time) => {
                            while (DateTime.UtcNow.Minute > 50) Thread.Sleep (1000);
                            //just in case
                            int pick = (DateTime.UtcNow.Hour + 8) % 12;
                            foreach (var i in alarmList) {
                                i.Item1.SendMessageAsync (i.Item2, new Message (
                                    new ElementImage (pics[pick])
                                ));
                            }
                        };
                        t.AutoReset = true;
                        t.Start ();
                    });
                    isStarted = true;
                }
                if (alarmList.Contains (p.endPoint) == false) {
                    alarmList.Add (p.endPoint);
                    return new cqhttp.Cyan.Messages.Message (new ElementText ("已添加到报时列表中"));
                }
                return new cqhttp.Cyan.Messages.Message (new ElementText ("本已在报时列表中"));
            });
            FunctionPool.onCommand.Add ("stop_alarm", (p) => {
                if (alarmList.Contains (p.endPoint))
                    alarmList.Remove (p.endPoint);
                return new cqhttp.Cyan.Messages.Message (new ElementText ("停止报时"));
            });
        }
        public static void UnloadModule () {
            FunctionPool.onCommand.Remove ("stop_alarm");
            FunctionPool.onCommand.Remove ("start_alarm");
            alarmList.Clear ();
        }
    }
}