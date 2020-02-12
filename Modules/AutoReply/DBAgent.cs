using System;
using System.Text.RegularExpressions;

namespace CyanBot.Modules.AutoReplyUtils {
    class DBAgent {
        public class Item {
            [LiteDB.BsonId]
            public int id { get; set; }
            public Regex keyword { get; set; }
            public string reply { get; set; }
            public string contributor { get; set; }
            public DateTime time { get; set; }
        }
        static LiteDB.LiteDatabase db = new LiteDB.LiteDatabase (
            "Filename=reply.db"
        );
        static LiteDB.LiteCollection<Item> col = db.GetCollection<Item> (
            "reply_items"
        );

        static DBAgent () {
        }
        public void Insert (string w, string d, string user) {
            col.Insert (new Item {
                keyword = new Regex (
                    w,
                    RegexOptions.Compiled | RegexOptions.Multiline,
                    TimeSpan.FromSeconds (1)
                ),
                reply = d,
                contributor = user,
                time = DateTime.Now
            });
        }
        public void Erase (string w) =>
            col.Delete (x => x.keyword.ToString () == w);
        public void Update (string w, string d, string user) {
            var o = col.FindOne (x => x.keyword.ToString () == w);
            o.reply = d;
            o.contributor = user;
            o.time = DateTime.Now;
            col.Update (o);
        }
        public bool isExist (string w) =>
            col.Exists (x => x.keyword.ToString () == w);
        public Item Lookup (string w) =>
            col.FindOne (x => x.keyword.IsMatch (w));
    }
}