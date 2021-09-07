using System;
using System.Collections.Generic;
using cqhttp.Cyan.Events.CQEvents.Base;
using cqhttp.Cyan.Messages;
using cqhttp.Cyan.Messages.CQElements;
using CyanBot.Modules.AutoReplyUtils;

namespace CyanBot.Modules {
    public class AutoReply : Module {
        static DBAgent db = new DBAgent ();
        public static Message Insert (string[] p, long sender, bool force = false) {
            if (p.Length != 2) {
                return new Message (new ElementText ("Usage: /teach {a} {b}\n其中a, b若含空格, 应用英文引号包括"));
            }
            if (p[0].Length < 1 || p[1].Length < 1) return new Message ();
            if (db.isExist (p[0])) {
                if (force)
                    db.Update (p[0], p[1], sender.ToString ());
                else
                    return new Message (new ElementText ("我已经被安排过这句话了！"));
            }
            db.Insert (p[0], p[1], sender.ToString ());
            return new Message (new ElementText ("谢谢你，我学会了，你呢?"));
        }
        public static Message Reply (string p) {
            if (p.Length == 0)
                throw new KeyNotFoundException ();
            var r = db.Lookup (p);
            if (r != null)
                return new Message (r.reply);
            throw new KeyNotFoundException ();
        }
        public static Message Delete (string p) {
            if (db.isExist (p)) {
                db.Erase (p);
                return new Message (new ElementText ("我。。忘了什么？"));
            }
            return new Message (new ElementText ("我本来就不知道这句话，那你叫我忘掉啥"));
        }

        [OnCommand ("teach")]
        public Message Teach (string[] parameters, MessageEvent e) =>
            Insert (parameters, e.sender.user_id);
        [OnCommand ("force")]
        public Message Force (string[] parameters, MessageEvent e) =>
            Insert (parameters, e.sender.user_id, true);
        [OnCommand ("reply")]
        public Message Reply (string[] parameters, MessageEvent e) {
            try {
                return Reply (parameters[0]);
            } catch (KeyNotFoundException) {
                return new Message (new ElementText ("我还不会这句话emmmmm..."));
            } catch {
                return new Message (new ElementText ("干嘛?"));
            }
        }

        [OnCommand ("delete")]
        public Message Delete (string[] parameters, MessageEvent e) {
            try {
                return Delete (parameters[0]);
            } catch {
                return new Message (new ElementText ("干嘛?"));
            }
        }

        [OnMessage]
        public Message ReplyTo (Message s, MessageEvent e) {
            try {
                string raw_text = "";
                foreach (var i in s.data)
                    if (i.type == "text") raw_text += i.data["text"];
                return Reply (raw_text);
            } catch (KeyNotFoundException) { }

            return new Message ();
        }
    }
}