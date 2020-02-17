using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using cqhttp.Cyan.Enums;
using cqhttp.Cyan.Events.CQEvents;
using cqhttp.Cyan.Events.CQEvents.Base;

namespace CyanBot {
    [AttributeUsage (AttributeTargets.Method, AllowMultiple = true)]
    public class OnCommandAttribute : Attribute {
        public string command;
        public OnCommandAttribute (string command) {
            this.command = command;
        }
    }

    [AttributeUsage (AttributeTargets.Method, AllowMultiple = true)]
    public class OnMessageAttribute : Attribute { }
    public static class Extensions {
        public static void Deconstruct (
            this string[] t,
            out string a, out string b
        ) {
            a = t[0];
            b = t[1];
        }
        public static IEnumerable<MethodInfo> GetMethodsBySig (this Type type, Type returnType, params Type[] parameterTypes) {
            return type.GetMethods ().Where ((m) => {
                if (m.ReturnType != returnType) return false;
                var parameters = m.GetParameters ();
                if ((parameterTypes == null || parameterTypes.Length == 0))
                    return parameters.Length == 0;
                if (parameters.Length != parameterTypes.Length)
                    return false;
                for (int i = 0; i < parameterTypes.Length; i++) {
                    if (parameters[i].ParameterType != parameterTypes[i])
                        return false;
                }
                return true;
            });
        }
        public static (MessageType, long) GetEndpoint (this MessageEvent e) {
            if (e is GroupMessageEvent) {
                return (
                    MessageType.group_,
                    (e as GroupMessageEvent).group_id
                );
            } else if (e is PrivateMessageEvent) {
                return (
                    MessageType.private_,
                    (e as PrivateMessageEvent).sender_id
                );
            } else if (e is DiscussMessageEvent) {
                return (
                    MessageType.discuss_,
                    (e as DiscussMessageEvent).discuss_id
                );
            } else return (MessageType.private_, 745679136);
        }
    }
}