using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using cqhttp.Cyan.Clients;
using cqhttp.Cyan.Events.CQEvents.Base;
using CyanBot.Modules;

namespace CyanBot.Dispatcher {
    class CommandErrorException : Exception { };
    public class Dispatcher {
        public async static Task Dispatch (
            CQApiClient cli,
            MessageEvent e
        ) {
            if (new List<string> (
                    System.IO.File.ReadLines ("bots")
                ).Contains (e.sender.user_id.ToString ())) {
                Console.Write (e.sender.user_id);
                Console.WriteLine (" is bot");
                return;
            }
            try {
                string raw_text = "";
                foreach (var i in e.message.data)
                    if (i.type == "text") raw_text += i.data["text"];
                var command = ParseCommand (raw_text);
                List<Task> to_run = new List<Task> ();
                if (await Module.loaded_modules.Values.AllAsync (async (mod) => {
                    var result = await mod.InvokeCommand (command.Item1, command.Item2, e);
                    if (result.data.Count == 0)
                        return true;
                    to_run.Append (cli.SendMessageAsync (e.GetEndpoint (), result));
                    return false;
                })) { throw new CommandErrorException (); }
                await Task.WhenAll (to_run);
            } catch (CommandErrorException) {
                foreach (var i in Module.loaded_modules.Values) {
                    var ret = i.InvokeMessage (e.message, e);
                    if (ret.data.Count != 0) //can respond
                        await cli.SendMessageAsync (e.GetEndpoint (), ret);
                }
            }
        }
        public static (string, string[]) ParseCommand (string raw) {
            try {
                if (raw.Split (' ') [0][0] != '/')
                    throw new CommandErrorException ();
            } catch {
                throw new CommandErrorException ();
            }
            string command = raw.Split (' ') [0].Substring (1);
            List<string> parameters = new List<string> ();
            if (raw.TrimEnd ().Length == command.Length)
                return (command, parameters.ToArray ());
            raw = raw.Substring (command.Length + 1).Trim ();
            for (int i = 0; i < raw.Length;) {
                int x = -1;
                if (raw[0] != '"') raw = raw.Insert (0, " ");
                switch (raw[i]) {
                case '"':
                    x = raw.Substring (i + 1).IndexOf ('"');
                    if (x == -1) throw new CommandErrorException ();
                    parameters.Add (raw.Substring (i + 1, x - i));
                    raw = raw.Substring (x + 2).Trim ();
                    i = 0;
                    break;
                case ' ':
                    x = raw.Substring (i + 1).Trim ().IndexOf (' ');
                    if (x == -1) {
                        parameters.Add (raw.Substring (i + 1).Trim ());
                        i = raw.Length; //break,break!
                    } else {
                        parameters.Add (raw.Substring (i + 1, x - i));
                        raw = raw.Substring (x + 1).Trim ();
                        i = 0;
                    }
                    break;
                }
            }
            return (command, parameters.ToArray ());
        }
    }
}