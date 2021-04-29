using System;
using System.Collections.Generic;
using System.IO;
using cqhttp.Cyan.Enums;
using cqhttp.Cyan.Events.CQEvents.Base;
using cqhttp.Cyan.Messages;
using ELIZA.NET;

namespace CyanBot.Modules {
    class ELIZABot : Module {
        readonly Dictionary<(MessageType, long), ELIZALib> eliza
            = new Dictionary<(MessageType, long), ELIZALib> ();

        [OnCommand ("eliza")]
        public Message Eliza (string[] parameters, MessageEvent e) {
            if (parameters.Length < 1)
                return new Message ();
            string profile = parameters.Length > 2 ? parameters[1] : "doctor";
            if (!File.Exists ("./eliza/" + profile + ".json"))
                return new Message ("no such profile");
            switch (parameters[0]) {
            case "start":
                using (Stream stream = new FileStream ("./eliza/" + profile + ".json", FileMode.Open)) {
                    using (StreamReader streamReader = new StreamReader (stream)) {
                        eliza.Add (e.GetEndpoint (), new ELIZALib (streamReader.ReadToEnd ()));
                    }
                }
                return new Message (eliza[e.GetEndpoint ()].Session.GetGreeting ());
            case "stop":
                string goodbye = eliza[e.GetEndpoint ()].Session.GetGoodbye ();
                eliza.Remove (e.GetEndpoint ());
                return new Message (goodbye);
            default:
                return new Message ();
            }
        }
        [OnMessage]
        public Message Reply (Message message, MessageEvent e) {
            if (!eliza.ContainsKey (e.GetEndpoint ())) return new Message ();
            return new Message (eliza[e.GetEndpoint ()].Session.GetResponse (message.GetRaw ()));
        }
    }
}