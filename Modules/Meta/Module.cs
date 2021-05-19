using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using cqhttp.Cyan.Events.CQEvents.Base;
using cqhttp.Cyan.Messages;

namespace CyanBot.Modules {
    public class Module {
        public static Dictionary<string, Module> loaded_modules =
            new Dictionary<string, Module> ();
        Dictionary<string, MethodInfo> on_commands =
            new Dictionary<string, MethodInfo> ();
        List<MethodInfo> on_messages = new List<MethodInfo> ();
        public Module () {
            var cmd_funcs = this.GetType ()
                .GetMethodsBySig (null,
                    typeof (string[]),
                    typeof (MessageEvent)
                );
            foreach (var func in cmd_funcs) {
                var cmd_attrs = func.GetCustomAttributes<OnCommandAttribute> (false);
                foreach (var attr in cmd_attrs)
                    on_commands.Add (attr.command, func);
            }
            var msg_funcs = this.GetType ()
                .GetMethodsBySig (null,
                    typeof (Message),
                    typeof (MessageEvent)
                );
            foreach (var func in msg_funcs) {
                var attr = func.GetCustomAttribute<OnMessageAttribute> (false);
                if (attr != null) on_messages.Add (func);
            }
            on_messages.Sort ((f1, f2) => {
                return
                    f1.GetCustomAttribute<OnMessageAttribute> (false).priority -
                    f2.GetCustomAttribute<OnMessageAttribute> (false).priority;
            });
        }
        public async Task<Message> InvokeCommand (
            string command, string[] parameter, MessageEvent raw_event
        ) {
            if (on_commands.ContainsKey (command)) {
                var result = on_commands[command].Invoke (
                    this, new object[] { parameter, raw_event }
                );
                if (on_commands[command].ReturnType == typeof (Task<Message>))
                    return await (Task<Message>) result;
                else return (Message) result;
            }
            return new Message ();
        }
        public async Task<Message> InvokeMessage (Message message, MessageEvent raw_event) {
            foreach (var func in on_messages) {
                if (func.GetCustomAttribute<OnMessageAttribute> (false)
                        .is_match (message.GetRaw ())
                    == false) continue;
                var result = func.Invoke (
                    this, new object[] { message, raw_event }
                );
                if (func.ReturnType == typeof (Task<Message>))
                    result = await (Task<Message>) result;
                if ((result as Message).data.Count > 0) return (Message) result;
            }
            return new Message ();
        }
    }
}