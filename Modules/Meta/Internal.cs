using System;
using System.Linq;
using System.Text;
using cqhttp.Cyan.Events.CQEvents.Base;
using cqhttp.Cyan.Messages;
using cqhttp.Cyan.Utils;

namespace CyanBot.Modules {
    class Internal : Module {
        Logger logger = Logger.GetLogger ("internal");
        class AdminRecord {
            [LiteDB.BsonId]
            public int id { get; set; }
            public long user_id { get; set; }
            public AdminRecord (long uid) => user_id = uid;
            public AdminRecord () { }
        }
        static readonly LiteDB.LiteCollection<AdminRecord> admins =
            Program.Data.GetCollection<AdminRecord> ("_internal_admin");

        class ModuleRecord {
            [LiteDB.BsonId]
            public int id { get; set; }
            public string module_name { get; set; }
            public ModuleRecord (string module) => module_name = module;
            public ModuleRecord () { }
        }
        static readonly LiteDB.LiteCollection<ModuleRecord> modules =
            Program.Data.GetCollection<ModuleRecord> ("_internal_modules");

        public Internal () {
            modules.FindAll ().Select (
                x => x.module_name
            ).All (x => TryLoadModule (x));
        }
        bool TryLoadModule (string module) {
            Type module_type = Type.GetType (this.GetType ().Namespace + '.' + module);
            if (module_type == null)
                return false;
            loaded_modules.Add (
                module,
                (Module) module_type
                    .GetConstructor (new Type[] { })
                    .Invoke (new object[] { })
            );
            return true;
        }

        [OnCommand ("get_loaded")]
        public Message GetLoaded (string[] parameters, MessageEvent e) {
            if (e.sender.user_id != long.Parse (
                    Program.Globals["super_user"]
                ) && admins.FindOne (x => x.user_id == e.sender.user_id) == null)
                return new Message ("Permission Denied");
            StringBuilder module_list = new StringBuilder ();
            foreach (var i in loaded_modules.Keys)
                module_list.AppendLine (i);
            return new Message (module_list.ToString ().TrimEnd ());
        }

        [OnCommand ("load_module")]
        public Message LoadModule (string[] parameters, MessageEvent e) {
            StringBuilder result = new StringBuilder ();
            if (e.sender.user_id != long.Parse (
                    Program.Globals["super_user"]
                ) && admins.FindOne (x => x.user_id == e.sender.user_id) == null)
                return new Message ("Permission Denied");
            foreach (var module in parameters) {
                if (loaded_modules.ContainsKey (module)) {
                    result.AppendLine ($"Already loaded {module}");
                    continue;
                }
                if (TryLoadModule (module)) {
                    modules.Insert (new ModuleRecord (module));
                    result.AppendLine ($"Loaded Module: {module}");
                    logger.Info ("Successfully Loaded: " + module);
                } else {
                    result.AppendLine ($"No such module {module}");
                }
            }
            return new Message (result.ToString ().TrimEnd ());
        }

        [OnCommand ("unload_module")]
        public Message UnloadModule (string[] parameters, MessageEvent e) {
            StringBuilder result = new StringBuilder ();
            if (e.sender.user_id != long.Parse (
                    Program.Globals["super_user"]
                ) && admins.FindOne (x => x.user_id == e.sender.user_id) == null)
                return new Message ("Permission Denied");
            foreach (var module in parameters) {
                if (loaded_modules.ContainsKey (module) == false)
                    return new Message ("Module not loaded");
                loaded_modules.Remove (module);
                modules.Delete (x => x.module_name == module);
                result.AppendLine ($"Loaded Module: {module}");
                logger.Info ("Successfully Unoaded: " + module);
            }
            return new Message (result.ToString ().TrimEnd ());
        }

        [OnCommand ("set_admin")]
        public Message SetAdmin (string[] parameters, MessageEvent e) {
            if (e.sender.user_id != long.Parse (Program.Globals["super_user"]))
                return new Message ("Permission Denied");
            foreach (var i in parameters) {
                long admin = long.Parse (i);
                if (admins.FindOne (x => x.user_id == e.sender.user_id) == null) {
                    admins.Insert (new AdminRecord (e.sender.user_id));
                }
            }
            return new Message ();
        }
    }
}
