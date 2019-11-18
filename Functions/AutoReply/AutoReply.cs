using System;
using System.Collections.Generic;
using cqhttp.Cyan.Enums;
using cqhttp.Cyan.Messages;
using cqhttp.Cyan.Messages.CQElements;
using CyanBot.Functions.AutoReplyUtils;

namespace CyanBot.Functions {
    public class AutoReply {
        public static Message Teach (List<string> p, long sender, bool force = false) {
            if (p.Count != 2) {
                return new Message (new ElementText ("Usage: /teach {a} {b}\n其中a, b若含空格, 应用英文引号包括"));
            }
            if (DBAgent.isExist (p[0])) {
                if (force)
                    DBAgent.Update (p[0], p[1], sender.ToString ());
                else
                    return new Message (new ElementText ("我已经被安排过这句话了！"));
            }
            DBAgent.Insert (p[0], p[1], sender.ToString ());
            return new Message (new ElementText ("谢谢你，我学会了，你呢?"));
        }
        public static Message Reply (string p) {
            if (DBAgent.isExist (p))
                return new Message (DBAgent.Lookup (p));
            throw new KeyNotFoundException ();
        }
        public static Message Delete (string p) {
            if (DBAgent.isExist (p)) {
                DBAgent.Erase (p);
                return new Message (new ElementText ("我。。忘了什么？"));
            }
            return new Message (new ElementText ("我本来就不知道这句话，那你叫我忘掉啥"));
        }
        static int onAllFuncIndex = 0;
        static bool isDBInitialized = false;
        public static void LoadModule () {
            if (!isDBInitialized)
                DBAgent.InitDB ();
            FunctionPool.onCommand.Add ("teach", (p) => Teach (p.parameters, p.sender.user_id));
            FunctionPool.onCommand.Add ("force", (p) => Teach (p.parameters, p.sender.user_id, true));
            FunctionPool.onCommand.Add ("reply", (p) => {
                try {
                    return Reply (p.parameters[0]);
                } catch (KeyNotFoundException) {
                    return new Message (new ElementText ("我还不会这句话emmmmm..."));
                } catch {
                    return new Message (new ElementText ("干嘛?"));
                }
            });
            FunctionPool.onCommand.Add ("delete", (p) => {
                try {
                    return Delete (p.parameters[0]);
                } catch {
                    return new Message (new ElementText ("干嘛?"));
                }
            });
            onAllFuncIndex = FunctionPool.onAny.Count;
            FunctionPool.onAny.Add ((e, s) => {
                try {
                    string raw_text = "";
                    foreach (var i in s.data)
                        if (i.type == "text") raw_text += i.data["text"];
                    if (Firewall.waf (e, raw_text))
                        return Reply (raw_text);
                    else {
                        Console.WriteLine ($"[WARNING] {e.Item2} triggered firewall");
                    }
                } catch (KeyNotFoundException) { }

                return new Message ();
            });
        }
        public static void UnloadModule () {
            foreach (var i in new List<string> () {
                    "teach",
                    "force",
                    "reply",
                    "delete"
                }) {
                FunctionPool.onCommand.Remove (i);
            }
            FunctionPool.onAny.RemoveAt (onAllFuncIndex);
        }
    }
}