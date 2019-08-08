using System.Collections.Generic;
using cqhttp.Cyan.Messages;
using cqhttp.Cyan.Messages.CQElements;

namespace CyanBot.Functions {
    public class AutoReply {
        public static Message Teach (List<string> p, bool force = false) {
            if (p.Count != 3) {
                return new Message (new ElementText ("Usage: /teach {a} {b}\n其中a, b若含空格, 应用英文引号包括"));
            }
            if (DBAgent.isExist (p[1])) {
                if (force)
                    DBAgent.Update (p[1], p[2], p[0]);
                else
                    return new Message (new ElementText ("我已经被安排过这句话了！"));
            }
            DBAgent.Insert (p[1], p[2], p[0]);
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
            if(!isDBInitialized)
                DBAgent.InitDB ();
            FunctionPool.onCommand.Add ("teach", (p) => Teach (p.parameters));
            FunctionPool.onCommand.Add ("force", (p) => Teach (p.parameters, true));
            FunctionPool.onCommand.Add ("reply", (p) => {
                try {
                    return Reply (p.parameters[1]);
                } catch (KeyNotFoundException) {
                    return new Message (new ElementText ("我还不会这句话emmmmm..."));
                } catch {
                    return new Message (new ElementText ("干嘛?"));
                }
            });
            FunctionPool.onCommand.Add ("delete", (p) => {
                try {
                    return Delete (p.parameters[1]);
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
                    return Reply (raw_text);
                } catch (KeyNotFoundException) {
                    return new Message ();
                }
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