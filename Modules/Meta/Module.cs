using System;
using System.Collections.Generic;
using System.Reflection;
using cqhttp.Cyan.Events.CQEvents.Base;
using cqhttp.Cyan.Messages;

namespace CyanBot.Modules {
    public class Module {
        public static Dictionary<string, Module> loaded_modules =
            new Dictionary<string, Module> (new KeyValuePair<string, Module>[] {
                new KeyValuePair<string, Module> ("Internal", new Internal ())
            });
        Dictionary<string, MethodInfo> on_commands =
            new Dictionary<string, MethodInfo> ();
        MethodInfo on_messages = null;
        public Module () {
            Console.WriteLine ("loading " + this.GetType ().Name);
            var cmd_funcs = this.GetType ()
                .GetMethodsBySig (
                    typeof (Message),
                    typeof (string),
                    typeof (string[]),
                    typeof (MessageEvent)
                );
            foreach (var func in cmd_funcs) {
                var cmd_attrs = (OnCommandAttribute[]) func.GetCustomAttributes (
                    typeof (OnCommandAttribute), false
                );
                foreach (var attr in cmd_attrs)
                    on_commands.Add (attr.command, func);
            }
            var msg_funcs = this.GetType ()
                .GetMethodsBySig (
                    typeof (Message),
                    typeof (Message),
                    typeof (MessageEvent)
                );;
            foreach (var func in msg_funcs) {
                var msg_attrs = (OnMessageAttribute) func.GetCustomAttribute (
                    typeof (OnMessageAttribute), false
                );
                if (msg_attrs != null)
                    on_messages = func;
            }
        }
        public Message InvokeCommand (
            string command, string[] parameter, MessageEvent raw_event
        ) {
            if (on_commands.ContainsKey (command)) {
                return (Message) on_commands[command].Invoke (this, new object[] {
                    command,
                    parameter,
                    raw_event
                });
            }
            return new Message ();
        }
        public Message InvokeMessage (Message message, MessageEvent raw_event) {
            if (on_messages == null) return new Message ();
            return (Message) on_messages.Invoke (this, new object[] { message, raw_event });
        }
    }
}