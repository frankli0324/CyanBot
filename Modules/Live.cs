using System.Text;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;
using cqhttp.Cyan.Events.CQEvents.Base;
using cqhttp.Cyan.Messages;
using Newtonsoft.Json.Linq;
using cqhttp.Cyan.Messages.CQElements;
using cqhttp.Cyan.Enums;

namespace CyanBot.Modules {
    class Live : Module {
        static HttpClient client = new HttpClient ();
        class Subscription {
            [LiteDB.BsonId]
            public int id { get; set; }
            public MessageType endpoint_type { get; set; }
            public long endpoint_num { get; set; }
            public long room { get; set; }
            public bool is_live { get; set; }
        }
        static readonly LiteDB.LiteCollection<Subscription> col =
            Program.Data.GetCollection<Subscription> ("subscriptions");
        readonly CancellationTokenSource token_source =
            new CancellationTokenSource ();

        [OnCommand ("live_subsc")]
        public Message Subscribe (string[] parameters, MessageEvent e) {
            if (!long.TryParse (parameters[0], out long id) || !long.TryParse (parameters[1], out long group))
                return new Message ();
            col.Insert (new Subscription {
                endpoint_num = group,
                endpoint_type = MessageType.group_,
                room = id,
                is_live = true,
            });
            return new Message ("subscribed");
        }

        [OnCommand ("live_unsubsc")]
        public Message Unsubscribe (string[] parameters, MessageEvent e) {
            if (!long.TryParse (parameters[0], out long id) || !long.TryParse (parameters[1], out long group))
                return new Message ();
            col.Delete (s => s.endpoint_num == group && s.room == id);
            return new Message ("unsubscribed");
        }

        [OnCommand ("live_list")]
        public Message List (string[] parameters, MessageEvent e) {
            StringBuilder builder = new StringBuilder ();
            foreach (var s in col.FindAll ()) {
                builder.Append ($"[{s.endpoint_num}] {s.room} {(s.is_live ? '✅' : '❌')}\n");
            }
            return new Message (builder.Remove (builder.Length - 1, 1).ToString ());
        }

        public Live () {
            var token = token_source.Token;
            Task.Run (async () => {
                while (token.IsCancellationRequested == false) {
                    await Task.Delay (TimeSpan.FromSeconds (8), token_source.Token);
                    if (token.IsCancellationRequested)
                        return;
                    foreach (var subsc in col.FindAll ()) {
                        try {
                            var info = JObject.Parse (await client.GetStringAsync (
                                $"https://api.live.bilibili.com/room/v1/Room/get_info?room_id={subsc.room}"
                            ));
                            if (info["code"].ToObject<int> () != 0) continue;
                            info = info["data"] as JObject;
                            if (info["live_status"].ToObject<int> () == 1) {
                                if (subsc.is_live) continue;
                                subsc.is_live = true;
                                col.Update (subsc);
                                await Program.client.SendMessageAsync (
                                    subsc.endpoint_type, subsc.endpoint_num,
                                    new Message (new ElementShare (
                                        $"https://live.bilibili.com/{subsc.room}",
                                        info["title"].ToObject<string> (),
                                        info["tags"].ToObject<string> (),
                                        info["background"].ToObject<string> ()
                                    ))
                                );
                            } else if (subsc.is_live) {
                                subsc.is_live = false;
                                col.Update (subsc);
                            }
                        } catch (Exception) { continue; }
                    }
                }
            }, token_source.Token);
        }
        ~Live () {
            token_source.Cancel ();
        }
    }
}