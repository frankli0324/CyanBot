using System.Threading;
using System.Threading.Tasks;
using cqhttp.Cyan.Events.CQEvents.Base;
using cqhttp.Cyan.Messages;
using System.Collections.Generic;
using cqhttp.Cyan.Enums;
using CyanBot.Modules.CTFdUtils;

namespace CyanBot.Modules {
    public class CTFd : Module {

        readonly Dictionary<(MessageType, long), CTFdClient> clients =
            new Dictionary<(MessageType, long), CTFdClient> ();
        readonly Dictionary<(MessageType, long), CancellationTokenSource> tokens =
            new Dictionary<(MessageType, long), CancellationTokenSource> ();

        [OnCommand ("monitor")]
        public Message Monitor (string[] parameters, MessageEvent e) {
            string host = parameters[0];
            var orig_client = clients.GetValueOrDefault (e.GetEndpoint (), null);
            if (orig_client != null) { orig_client.Dispose (); }
            var orig_token = tokens.GetValueOrDefault (e.GetEndpoint (), null);
            if (orig_token != null) { orig_token.Cancel (); orig_token.Dispose (); }
            tokens[e.GetEndpoint ()] = new CancellationTokenSource ();
            clients[e.GetEndpoint ()] = new CTFdClient (host);
            Task.Run (async () => {
                await clients[e.GetEndpoint ()].Login (parameters[1], parameters[2]);
                var client = clients[e.GetEndpoint ()];
                await foreach (var i in clients[e.GetEndpoint ()].GetEvents ()) {
                    var message = Get.Message (i);
                    if (message != null)
                        await Program.client.SendMessageAsync (
                            e.GetEndpoint (), message
                        );
                }
                ;// syntax highlighting bug
            }, tokens[e.GetEndpoint ()].Token);
            return new Message ("started monitoring " + host);
        }
        [OnCommand ("cancel_monitor")]
        public Message Purge (string[] parameters, MessageEvent e) {
            if (tokens.ContainsKey (e.GetEndpoint ()))
                tokens[e.GetEndpoint ()].Cancel ();
            return new Message ();
        }

        [OnCommand ("ctfd_log_level")]
        public Message LogLevel (string[] parameters, MessageEvent e) {
            var logger = cqhttp.Cyan.Utils.Logger.GetLogger ("CTFd");
            if(parameters[0] == "debug")
                logger.log_level = Verbosity.DEBUG;
            else if(parameters[0] == "info")
                logger.log_level = Verbosity.INFO;
            return new Message ();
        }
    }
}