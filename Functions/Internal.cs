using System;
using System.Collections.Generic;
using cqhttp.Cyan.Messages;

namespace CyanBot.Functions {
    class Internal {
        public static Dictionary<string, bool> isModuleLoaded =
            new Dictionary<string, bool> ();
        public static Dictionary<string, Type> moduleTypes =
            new Dictionary<string, Type> ();
        public static void LoadModule () {
            FunctionPool.onCommand.Add ("get_loaded", (cmd) => {
                if (cmd.sender.user_id != Config.super_user)
                    return new Message ("Permission Denied");
                string module_list = "";
                foreach (var i in isModuleLoaded.Keys)
                    module_list += i + ": " + (isModuleLoaded[i] ? "Loaded" : "Unloaded") + '\n';
                return new Message (module_list.TrimEnd ('\n'));
            });
            FunctionPool.onCommand.Add ("load_module", (cmd) => {
                string module = cmd.parameters[0];
                if (cmd.sender.user_id != Config.super_user)
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
                if (cmd.sender.user_id != Config.super_user)
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
        public static void UnloadModule () {
            // does nothing
        }
    }
}