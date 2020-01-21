using System;
using System.Collections.Generic;
using System.Linq;
using cqhttp.Cyan.Events.CQEvents.Base;
using cqhttp.Cyan.Messages;
using cqhttp.Cyan.Messages.CQElements;

/*
inspired by https://coin.keybrl.com/
*/
namespace CyanBot.Modules {
    public class Choice : Module {
        static T RandomChoice<T> (IEnumerable<T> source) {
            if (source == null) {
                throw new ArgumentNullException ("source");
            }

            var list = source.ToList ();

            if (list.Count < 1) {
                throw new MissingMemberException ();
            }

            var rnd = new Random ();
            return list[rnd.Next (0, list.Count)];
        }
        static string GetEmptyClassRoom (string building) {
            return "";
        }
        static string GetSeat () {
            int row = RandomChoice (Enumerable.Range (2, 18));
            int col = RandomChoice (Enumerable.Range (1, 16));
            return $"第{row}排第{col}列";
        }
        static string GetEat () {
            bool goto_comm = RandomChoice (new List<bool> { true, false });
            string result = "";
            string floor = RandomChoice (new List<string> {
                "一楼",
                "二楼"
            });
            if (goto_comm) {
                string restaurant = RandomChoice (new List<string> {
                    "老综合楼",
                    "新综合楼"
                });
                result = restaurant + floor;
            } else {
                string restaurant = RandomChoice (new List<string> {
                    "海棠餐厅",
                    "丁香餐厅",
                    "竹园餐厅"
                });
                result = restaurant + RandomChoice (new List<string> {
                    "靠西侧的窗口",
                    "靠东侧的窗口",
                    "中间的窗口"
                });
            }
            return result;
        }

        [OnCommand ("roll")]
        public Message Roll (string[] parameters, MessageEvent e) {
            string result;
            if (parameters.Length == 0) result = "rollable: 座位/吃饭";
            else
                switch (parameters[0]) {
                case "座位":
                    result = GetSeat ();
                    break;
                case "吃饭":
                    result = GetEat ();
                    break;
                default:
                    result = "rollable: 座位/吃饭";
                    break;
                }
            return new Message (new ElementText (
                result
            ));
        }
    }
}