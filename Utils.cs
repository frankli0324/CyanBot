using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CyanBot {
    [AttributeUsage (AttributeTargets.Method, AllowMultiple = true)]
    public class OnCommandAttribute : Attribute {
        public string command;
        public OnCommandAttribute (string command) {
            this.command = command;
        }
    }

    [AttributeUsage (AttributeTargets.Method, AllowMultiple = false)]
    public class OnMessageAttribute : Attribute {
        public enum Mode { keyword, regex, starts_with, ends_with, none }
        public Func<string, bool> is_match;
        public int priority;
        public OnMessageAttribute (string keyword = null, Mode mode = Mode.none, int priority = 1) {
            Regex pattern = mode == Mode.regex ? new Regex (keyword) : null;
            this.priority = priority;
            switch (mode) {
            case Mode.keyword: is_match = (x) => x.Contains (keyword); break;
            case Mode.regex: is_match = (x) => pattern.IsMatch (keyword); break;
            case Mode.starts_with: is_match = (x) => x.StartsWith (keyword); break;
            case Mode.ends_with: is_match = (x) => x.EndsWith (keyword); break;
            default: is_match = (x) => true; break;
            }
        }
    }

    public class TimerAsync {
        public Func<Task> job;
        Task work;
        public delegate Task Job ();
        public event Job Elapsed;
        public TimerAsync (int cycle, CancellationToken token) {
            work = Task.Run (async () => {
                while (true) {
                    if (token.IsCancellationRequested) break;
                    await Elapsed ();
                    await Task.Delay (cycle, token);
                }
            });
        }
    }
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
                if (returnType != null && m.ReturnType != returnType) return false;
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
        public static async System.Threading.Tasks.Task<bool> AllAsync<TSource> (this IEnumerable<TSource> source, Func<TSource, System.Threading.Tasks.Task<bool>> predicate) {
            if (source == null)
                throw new ArgumentNullException (nameof (source));
            if (predicate == null)
                throw new ArgumentNullException (nameof (predicate));
            foreach (var item in source) {
                var result = await predicate (item);
                if (!result)
                    return false;
            }
            return true;
        }
        public static string GetRaw (this cqhttp.Cyan.Messages.Message e) {
            string raw_text = "";
            foreach (var i in e.data)
                if (i.type == "text") raw_text += i.data["text"];
            return raw_text;
        }
    }
}