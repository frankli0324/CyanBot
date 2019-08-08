using System;
using System.Collections.Generic;
using cqhttp.Cyan.Enums;
using cqhttp.Cyan.Messages;
using cqhttp.Cyan.Utils;

namespace CyanBot.Functions {
    public class Advertisement {
        static string getText (Message m) {
            string ret = "";
            foreach (var i in m.data) {
                if (i.type == "text")
                    ret += i.data["text"];
            }
            return ret;
        }

        static Dictionary<long, long> cnt_received =
            new Dictionary<long, long> (), target_received =
            new Dictionary<long, long> ();
        static Dictionary < (MessageType, long), List<string>> cache =
            new Dictionary < (MessageType, long), List<string>> ();

        static Dictionary<long, Message> task_message =
            new Dictionary<long, Message> ();

        static int onAllFuncIndex = 0;
        static Dialogue buildDialogue (Dispatcher.Command parameters) {
            Dialogue dialogue = new Dialogue ();
            dialogue["BEGIN"] = (cli, msg) => {
                cache[parameters.endPoint.Item2] = new List<string> (getText (msg).Split (','));
                cli.SendTextAsync (parameters.endPoint.Item2, "请发送需要发送的消息");
                return "NEXT";
            };
            dialogue["NEXT"] = (cli, msg) => {
                try {
                    foreach (string i in cache[parameters.endPoint.Item2])
                        task_message[long.Parse (i)] = msg;
                } catch (FormatException) {
                    cli.SendTextAsync (parameters.endPoint.Item2, "群号必须为整数，请重新发送群号");
                    return "BEGIN";
                }
                cli.SendTextAsync (parameters.endPoint.Item2, "请发送间隔(每隔多少条消息发送一次)");
                return "COUNT";
            };
            dialogue["COUNT"] = (cli, msg) => {
                try {
                    long target = int.Parse (getText (msg));
                    foreach (string i in cache[parameters.endPoint.Item2]) {
                        target_received[long.Parse (i)] = target;
                        cnt_received[long.Parse (i)] = 0;
                    }
                } catch {
                    cli.SendTextAsync (parameters.endPoint.Item2, "请输入一个整数");
                    return "COUNT";
                }
                cache.Remove (parameters.endPoint.Item2);
                cli.SendTextAsync (parameters.endPoint.Item2, "已添加");
                return "DONE";
            };
            return dialogue;
        }
        public static void LoadModule () {
            FunctionPool.onCommand.Add ("add_task", (p) => {
                p.endPoint.Item1.SendTextAsync (p.endPoint.Item2, "请输入要发送消息的群聊号码\n如果需要向多个群发送，用英文逗号隔开群号");
                throw new InvokeDialogueException (buildDialogue (p));
            });
            FunctionPool.onCommand.Add ("clear_tasks", (p) => {
                task_message.Clear ();
                cnt_received.Clear ();
                target_received.Clear ();
                return new Message ("已清除所有定时消息");
            });
            onAllFuncIndex = FunctionPool.onAny.Count;
            FunctionPool.onAny.Add ((endpoint, msg) => {
                if (endpoint.Item1 != MessageType.group_)
                    return new Message ();
                if (!cnt_received.ContainsKey (endpoint.Item2))
                    return new Message ();

                cnt_received[endpoint.Item2]++;
                if (cnt_received[endpoint.Item2] ==
                    target_received[endpoint.Item2]) {
                    cnt_received[endpoint.Item2] = 0;
                    return task_message[endpoint.Item2];
                }
                return new Message ();
            });
        }
        public static void UnloadModule () {
            FunctionPool.onAny.RemoveAt (onAllFuncIndex);
            foreach (var i in new List<string> () { "add_task", "clear_tasks" }) {
                FunctionPool.onCommand.Remove (i);
            }
        }
    }
}