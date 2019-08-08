using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using cqhttp.Cyan.Enums;
using cqhttp.Cyan.Messages;

namespace CyanBot {
    public static class FunctionPool {
        public static Dictionary<string, Func<Dispatcher.Command, Message>> onCommand =
            new Dictionary<string, Func<Dispatcher.Command, Message>> ();
        public static List < Func < (MessageType, long), Message, Message >> onAny =
            new List < Func < (MessageType, long), Message, Message >> ();
        public static void Initialize () {
            var q = from t in Assembly.GetExecutingAssembly ().GetTypes ()
            where t.IsClass && t.Namespace == "CyanBot.Functions"
            select t;
            foreach (var i in q.ToList ()) {
                var RegFunc = i.GetMethod ("LoadModule");
                if (RegFunc == null || i.GetMethod ("UnloadModule") == null) continue;
                if (!RegFunc.IsStatic || RegFunc.IsAbstract || !RegFunc.IsPublic) continue;
                CyanBot.Functions.Internal.isModuleLoaded.Add (i.Name, true);
                CyanBot.Functions.Internal.moduleTypes.Add (i.Name, i);
                cqhttp.Cyan.Logger.Info ("CyanBot: Loading Module: " + i.Name);
                RegFunc.Invoke (null, null);
            }
        }
    }
}