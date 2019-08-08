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
        static Dictionary<string, bool> isModuleLoaded =
            new Dictionary<string, bool> ();
        static Dictionary<string, Type> moduleTypes =
            new Dictionary<string, Type> ();
        public static void Initialize () {
            var q = from t in Assembly.GetExecutingAssembly ().GetTypes ()
            where t.IsClass && t.Namespace == "CyanBot.Functions"
            select t;
            foreach (var i in q.ToList ()) {
                var RegFunc = i.GetMethod ("LoadModule");
                if (RegFunc == null) continue;
                if (!RegFunc.IsStatic || RegFunc.IsAbstract || !RegFunc.IsPublic) continue;
                isModuleLoaded.Add (i.Name, true);
                moduleTypes.Add (i.Name, i);
                cqhttp.Cyan.Logger.Info ("CyanBot: Loading Module: " + i.Name);
                RegFunc.Invoke (null, null);
            }
            FunctionPool.onCommand.Add ("get_loaded", (cmd) => {
                if (cmd.endPoint.Item2.Item2 != Config.super_user)
                    return new Message ("Permission Denied");
                string module_list = "";
                foreach (var i in isModuleLoaded.Keys)
                    module_list += i + ": " + (isModuleLoaded[i] ? "Loaded" : "Unloaded") + '\n';
                return new Message (module_list.TrimEnd ('\n'));
            });
            FunctionPool.onCommand.Add ("load_module", (cmd) => {
                string module = cmd.parameters[0];
                if (cmd.endPoint.Item2.Item2 != Config.super_user)
                    return new Message ("Permission Denied");
                if (!moduleTypes.ContainsKey (module))
                    return new Message ($"No Such Module: \"{module}\"");
                if (isModuleLoaded[module])
                    return new Message ("Module Already Loaded");
                moduleTypes[module].GetMethod ("LoadModule").Invoke (null, null);
                isModuleLoaded[module] = true;
                return new Message ($"Loaded Module: \"{module}\"");
            });
            FunctionPool.onCommand.Add ("unload_module", (cmd) => {
                string module = cmd.parameters[0];
                if (cmd.endPoint.Item2.Item2 != Config.super_user)
                    return new Message ("Permission Denied");
                if (!moduleTypes.ContainsKey (module))
                    return new Message ($"No Such Module: \"{module}\"");
                if (!isModuleLoaded[module])
                    return new Message ("Module Not Loaded");
                moduleTypes[module].GetMethod ("UnloadModule").Invoke (null, null);
                isModuleLoaded[module] = false;
                return new Message ($"Unloaded Module: \"{module}\"");
            });
        }
    }
}