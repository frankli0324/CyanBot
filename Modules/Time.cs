using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using cqhttp.Cyan.Enums;
using cqhttp.Cyan.Events.CQEvents.Base;
using cqhttp.Cyan.Messages;
using cqhttp.Cyan.Messages.CQElements;

namespace CyanBot.Modules {
    public class Time : Module {
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
        HashSet < (MessageType, long) > alarmList =
            new HashSet < (MessageType, long) > ();
        bool isStarted = false;
        System.Timers.Timer t = new System.Timers.Timer (3600000);

        [OnCommand ("start_alarm")]
        public Message StartAlarm (string cmd, string[] parameters, MessageEvent e) {
            if (isStarted == false) {
                Task.Run (() => {
                    var next_hour = DateTime.UtcNow.AddHours (1);
                    next_hour = next_hour.AddMinutes (-next_hour.Minute);
                    next_hour = next_hour.AddSeconds (-next_hour.Second);
                    Thread.Sleep (next_hour - DateTime.UtcNow);
                    t.Elapsed += (o, time) => {
                        while (DateTime.UtcNow.Minute > 50) Thread.Sleep (1000);
                        //just in case
                        int pick = (DateTime.UtcNow.Hour + 8) % 12;
                        foreach (var i in alarmList) {
                            Program.client.SendMessageAsync (i, new Message (
                                new ElementImage (pics[pick])
                            ));
                        }
                    };
                    t.AutoReset = true;
                    t.Start ();
                });
                isStarted = true;
            }
            if (alarmList.Contains (e.GetEndpoint ()) == false) {
                alarmList.Add (e.GetEndpoint ());
                return new Message (new ElementText ("已添加到报时列表中"));
            }
            return new Message (new ElementText ("本已在报时列表中"));
        }

        [OnCommand ("stop_alarm")]
        public Message StopAlarm (string cmd, string[] parameters, MessageEvent e) {
            if (alarmList.Contains (e.GetEndpoint ()))
                alarmList.Remove (e.GetEndpoint ());
            return new cqhttp.Cyan.Messages.Message (new ElementText ("停止报时"));
        }
    }
}