using System;
using System.Data.SQLite;
using System.IO;
using System.Text;

namespace CyanBot.Modules.AutoReplyUtils {
    class DBAgent {
        SQLiteConnection connection;
        public DBAgent (string file) {
            if (File.Exists (file) == false)
                SQLiteConnection.CreateFile (file);
            connection = new SQLiteConnection (
                "DataSource=" + file
            );
            connection.Open ();
            using (SQLiteCommand command = new SQLiteCommand (connection)) {
                command.CommandText =
                    "CREATE TABLE IF NOT EXISTS data " +
                    "(key text, val text, created_by text, last_edit text)";
                command.ExecuteNonQuery ();
            }
        }
        public void Insert (string w, string d, string user) {
            string aw = Convert.ToBase64String (Encoding.UTF8.GetBytes (w));
            string ad = Convert.ToBase64String (Encoding.UTF8.GetBytes (d));
            using (SQLiteCommand command = new SQLiteCommand (connection)) {
                command.CommandText =
                    "INSERT INTO data VALUES " +
                    "(@KEY, @VAL, @USR, @LST)";
                command.Parameters.AddRange (new SQLiteParameter[] {
                    new SQLiteParameter ("@KEY", w),
                        new SQLiteParameter ("@VAL", d),
                        new SQLiteParameter ("@USR", user),
                        new SQLiteParameter ("@LST", DateTime.Now.ToFileTimeUtc ().ToString ())
                });
                command.ExecuteNonQuery ();
            }
        }
        public void Erase (string w) {
            string aw = Convert.ToBase64String (Encoding.UTF8.GetBytes (w));
            using (SQLiteCommand command = new SQLiteCommand (connection)) {
                command.CommandText = "DELETE FROM data WHERE key=@KEY";
                command.Parameters.AddWithValue ("@KEY", w);
                command.ExecuteNonQuery ();
            }
        }
        public void Update (string w, string d, string user) {
            string aw = Convert.ToBase64String (Encoding.UTF8.GetBytes (w));
            string ad = Convert.ToBase64String (Encoding.UTF8.GetBytes (d));
            using (SQLiteCommand command = new SQLiteCommand (connection)) {
                command.CommandText =
                    "UPDATE data SET " +
                    "val=@VAL, created_by=@USR, last_edit=@LST " +
                    "WHERE key=@KEY";
                command.Parameters.AddRange (new SQLiteParameter[] {
                    new SQLiteParameter ("@KEY", w),
                        new SQLiteParameter ("@VAL", d),
                        new SQLiteParameter ("@USR", user),
                        new SQLiteParameter ("@LST", DateTime.Now.ToFileTimeUtc ().ToString ())
                });
                command.ExecuteNonQuery ();
            }
        }
        public bool isExist (string w) {
            string aw = Convert.ToBase64String (Encoding.UTF8.GetBytes (w));
            using (SQLiteCommand command = new SQLiteCommand (connection)) {
                command.CommandText =
                    "SELECT true WHERE EXISTS (SELECT * FROM data WHERE key=@KEY)";
                command.Parameters.AddWithValue ("@KEY", w);
                return command.ExecuteReader ().Read ();
            }
        }
        public string Lookup (string w) {
            using (SQLiteCommand command = new SQLiteCommand (connection)) {
                command.CommandText =
                    "SELECT val FROM data " +
                    "WHERE key=@KEY";
                command.Parameters.AddWithValue ("@KEY", w);
                var reader = command.ExecuteReader ();
                reader.Read ();
                return reader.GetString (0);
            }
        }
    }
}