using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;

namespace CyanBot.Functions.AutoReplyUtils {
    class DBAgent {

        private static SQLHelper data;
        public static void InitDB () {
            if (File.Exists ("reply.db") == false)
                SQLiteConnection.CreateFile ("reply.db");
            data = new SQLHelper ("data source=reply.db;");
            data.CreateTable (
                "data",
                new string[] { "key", "val", "create_by", "last_edit" },
                new string[] { "text", "text", "text", "text" }
            );
        }
        public static void Insert (string w, string d, string user) {
            string aw = Convert.ToBase64String (Encoding.UTF8.GetBytes (w));
            string ad = Convert.ToBase64String (Encoding.UTF8.GetBytes (d));
            data.InsertValues ("data", new string[] { aw, ad, user, "" });
        }
        public static void Erase (string w) {
            string aw = Convert.ToBase64String (Encoding.UTF8.GetBytes (w));
            data.DeleteValuesAND (
                "data",
                new string[] { "key" },
                new string[] { aw },
                new string[] { "=" }
            );
        }
        public static void Update (string w, string d, string user) {
            string aw = Convert.ToBase64String (Encoding.UTF8.GetBytes (w));
            string ad = Convert.ToBase64String (Encoding.UTF8.GetBytes (d));
            data.UpdateValues ("data",
                new string[] { "val", "last_edit" }, new string[] { ad, user }, "key", aw);
        }
        public static bool isExist (string w) { //emmmmm这里写的丑是因为刚才没仔细想sql该咋写
            string aw = Convert.ToBase64String (Encoding.UTF8.GetBytes (w));
            SQLiteDataReader d = data.ReadTable (
                "data",
                new string[] { "val" },
                new string[] { "key" },
                new string[] { "=" },
                new string[] { $"\'{aw}\'" }
            );
            if (d.Read ()) { return true; } else { return false; }
        }
        public static string Lookup (string w) {
            string aw = Convert.ToBase64String (Encoding.UTF8.GetBytes (w));
            SQLiteDataReader d = data.ReadTable (
                "data",
                new string[] { "val" },
                new string[] { "key" },
                new string[] { "=" },
                new string[] { $"\'{aw}\'" }
            );
            string ret = "";
            while (d.Read ())
                ret = d["val"].ToString ();
            ret = Encoding.UTF8.GetString (Convert.FromBase64String (ret));
            return ret;
        }
    }
}