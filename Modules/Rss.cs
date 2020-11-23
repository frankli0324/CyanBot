using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using CodeHollow.FeedReader;
using cqhttp.Cyan.Enums;
using cqhttp.Cyan.Events.CQEvents.Base;
using cqhttp.Cyan.Messages;
using cqhttp.Cyan.Messages.CQElements;

namespace CyanBot.Modules {
    public class Rss : Module {
        public class Subscription {
            [LiteDB.BsonId]
            public int id { get; set; }
            public MessageType endpoint_type { get; set; }
            public long endpoint_num { get; set; }
            public string feed { get; set; }
            public string last_id { get; set; }
        }
        static object round_lock = new object ();
        Timer t = new Timer ((o) => {
            lock (round_lock) {
                Task.WaitAll (col.FindAll ().Select (
                    x => FeedReader.ReadAsync (x.feed)
                    .ContinueWith (async (feed_task, o) => {
                        var feed = feed_task.Result.Items.First ();
                        if (feed.Id == x.last_id) return;
                        x.last_id = feed.Id;
                        col.Update (x);
                        await Program.client.SendMessageAsync (
                            x.endpoint_type, x.endpoint_num,
                            Program.client.is_pro ?
                            new Message (new ElementShare (
                                feed.Link, feed.Title, feed.Description, feed_task.Result.ImageUrl
                            )) : new Message (string.Join ('\n', feed.Title, feed.Link))
                        );
                    }, null)
                ).ToArray ());
            }
        }, null, 0, 10000);
        static readonly LiteDB.LiteCollection<Subscription> col =
            Program.Data.GetCollection<Subscription> ("subscriptions");

        [OnCommand ("subsc")]
        [OnCommand ("subscribe")]
        public Message Subscribe (string[] parameters, MessageEvent e) {
            if (!parameters.Any ()) return new Message ("/subscribe [feed]");
            try {
                var subscribed = col.Find (x =>
                    e.GetEndpoint ().Item1 == x.endpoint_type &&
                    e.GetEndpoint ().Item2 == x.endpoint_num
                );
                if (subscribed.Any (x => x.feed == parameters[0]))
                    return new Message ("already subscribed");
                FeedReader.ReadAsync (parameters[0]).ContinueWith (
                    (f, o) =>
                        col.Insert (new Subscription {
                            endpoint_type = e.GetEndpoint ().Item1,
                            endpoint_num = e.GetEndpoint ().Item2,
                            feed = parameters[0],
                            last_id = ""
                        })
                    , null
                );
            } catch {
                return new Message ("failed to subscribe");
            }
            return new Message ("subscribed");
        }

        [OnCommand ("unsubsc")]
        public Message Unsubscribe (string[] parameters, MessageEvent e) {
            if (parameters.Length == 0) return new Message ("/unsubscribe [feed]");
            var obj = col.FindOne (x =>
                x.endpoint_type == e.GetEndpoint ().Item1 &&
                x.endpoint_num == e.GetEndpoint ().Item2 &&
                x.feed == parameters[0]
            );
            if (obj != null) {
                col.Delete (x => x.id == obj.id);
                return new Message ("unsubscribed");
            }
            return new Message ("not subscribed yet");
        }

        [OnCommand ("get_subscribed")]
        public Message List (string[] parameters, MessageEvent e) =>
            new Message (string.Join (
                '\n', col.Find (x =>
                    x.endpoint_type == e.GetEndpoint ().Item1 &&
                    x.endpoint_num == e.GetEndpoint ().Item2
                ).Select (x => x.feed)
            ));
    }
}