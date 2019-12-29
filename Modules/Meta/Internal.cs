using System;
using System.Collections.Generic;
using System.Text;
using cqhttp.Cyan.Events.CQEvents.Base;
using cqhttp.Cyan.Messages;

namespace CyanBot.Modules {
    class Internal : Module {
        List<long> admins = new List<long> ();
        [OnCommand ("get_loaded")]
        public Message GetLoaded (string cmd, string[] parameters, MessageEvent e) {
            if (e.sender.user_id != Config.super_user && admins.Contains (e.sender.user_id) == false)
                return new Message ("Permission Denied");
            StringBuilder module_list = new StringBuilder ();
            foreach (var i in loaded_modules.Keys)
                module_list.AppendLine (i);
            return new Message (module_list.ToString ().TrimEnd ());
        }

        [OnCommand ("load_module")]
        public Message LoadModule (string cmd, string[] parameters, MessageEvent e) {
            string module = parameters[0];
            if (e.sender.user_id != Config.super_user && admins.Contains (e.sender.user_id) == false)
                return new Message ("Permission Denied");
            if (loaded_modules.ContainsKey (module))
                return new Message ("Already loaded");
            Type module_type = Type.GetType (this.GetType ().Namespace + '.' + module);
            if (module_type == null)
                return new Message ("No such module");
            loaded_modules.Add (
                module,
                (Module) module_type
                .GetConstructor (new Type[] { })
                .Invoke (new object[] { })
            );
            return new Message ($"Loaded Module: \"{module}\"");
        }

        [OnCommand ("unload_module")]
        public Message UnloadModule (string cmd, string[] parameters, MessageEvent e) {
            string module = parameters[0];
            if (e.sender.user_id != Config.super_user && admins.Contains (e.sender.user_id) == false)
                return new Message ("Permission Denied");
            if (loaded_modules.ContainsKey (module) == false)
                return new Message ("Module not loaded");
            loaded_modules.Remove (module);
            return new Message ($"Unloaded Module: \"{module}\"");
        }

        [OnCommand ("set_admin")]
        public Message SetAdmin (string cmd, string[] parameters, MessageEvent e) {
            if (e.sender.user_id != Config.super_user)
                return new Message ("Permission Denied");
            foreach (var i in parameters) {
                long admin = long.Parse (i);
                if (admins.Contains (admin) == false) {
                    admins.Add (admin);
                }
            }
            return new Message ();
        }
    }
}