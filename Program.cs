using System;
using System.Collections.Generic;
using System.Threading;
using cqhttp.Cyan.Clients;
using cqhttp.Cyan.Events.CQEvents.Base;
using cqhttp.Cyan.Events.CQResponses;

namespace CyanBot {
    class Program {
        public static CQApiClient client;
        public static Dictionary<string, string> Globals =
            new Dictionary<string, string> ();
        public static readonly LiteDB.LiteDatabase Data =
            new LiteDB.LiteDatabase ("Filename=cyan.db");
        static void Main (string[] args) {
            foreach (System.Collections.DictionaryEntry obj in Environment.GetEnvironmentVariables ())
                Globals.Add ((string) obj.Key, (string) obj.Value);
            client = new CQWebsocketClient (
                access_url: Globals["access_url"],
                event_url: Globals["event_url"],
                access_token: Globals["access_token"]
            );
            Modules.Module.loaded_modules.Add ("Internal", new Modules.Internal ());

            cqhttp.Cyan.Utils.Logger.GetLogger ("cqhttp.Cyan").log_level = cqhttp.Cyan.Enums.Verbosity.ERROR;
            cqhttp.Cyan.Utils.Logger.GetLogger ("internal").log_level = cqhttp.Cyan.Enums.Verbosity.INFO;

            client.OnEventAsync += async (cli, e) => {
                if (e is MessageEvent mevent)
                    await Dispatcher.Dispatcher.Dispatch (cli, mevent);
                return new EmptyResponse ();
            };
            Thread.Sleep (Timeout.Infinite);
        }
    }
}
