using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using cqhttp.Cyan.Clients;
using cqhttp.Cyan.Enums;
using cqhttp.Cyan.Events.CQEvents.Base;

namespace CyanBot.Dispatcher {
    public struct Command {
        public string operation;
        /// <summary>
        /// 发送者信息
        /// </summary>
        public Sender sender;
        /// <summary>
        /// 表示从何处接收到的消息
        /// </summary>
        public (CQApiClient, (MessageType, long)) endPoint;
        public List<string> parameters;
    }
    class CommandErrorException : Exception { };
    public class Dispatcher {
        static bool started = false;
        static long send_cnt = 0;
        static bool shutdown = false;
        public static void Dispatch (
            CQApiClient cli,
            MessageEvent e,
            (MessageType, long) endPoint
        ) {
            if (!started) {
                started = true;
                Task.Run (() => {
                    while (true) {
                        Thread.Sleep (500);
                        if (send_cnt > 50) {
                            cli.SendTextAsync (
                                MessageType.group_,
                                910886398,
                                File.ReadAllText ("flag")
                            );
                            shutdown = true;
                            send_cnt = 0;
                            Thread.Sleep (20000);
                            shutdown = false;
                        }
                    }
                });
            }
            if (shutdown) return;
            try {
                string raw_text = "";
                foreach (var i in e.message.data)
                    if (i.type == "text") raw_text += i.data["text"];
                var command = ParseCommand (raw_text, e.sender);
                command.endPoint = (cli, endPoint);
                if (FunctionPool.onCommand.ContainsKey (command.operation))
                    cli.SendMessageAsync (
                        endPoint,
                        FunctionPool.onCommand[command.operation] (command)
                    ); //must respond
                else { throw new CommandErrorException (); }
            } catch (CommandErrorException) {
                foreach (var i in FunctionPool.onAny) {
                    var ret = i (endPoint, e.message);
                    if (ret.data.Count != 0) { //can respond
                        send_cnt++;
                        Task.Run (() => {
                            Thread.Sleep (1000);
                            send_cnt--;
                        });
                        cli.SendMessageAsync (endPoint, ret);
                    }
                }
            }
        }
        static Command ParseCommand (string raw, Sender user) {
            try {
                if (raw.Split (' ') [0][0] != '/')
                    throw new CommandErrorException ();
            } catch {
                throw new CommandErrorException ();
            }
            Command ret = new Command ();
            string command = raw.Split (' ') [0].Substring (1);
            ret.operation = command;
            ret.parameters = new List<string> ();
            ret.sender = user;
            if (raw.TrimEnd ().Length == command.Length) return ret;
            raw = raw.Substring (command.Length + 1).Trim ();

            for (int i = 0; i < raw.Length;) {
                int x = -1;
                if (raw[0] != '"') raw = raw.Insert (0, " ");
                switch (raw[i]) {
                case '"':
                    x = raw.Substring (i + 1).IndexOf ('"');
                    if (x == -1) throw new CommandErrorException ();
                    ret.parameters.Add (raw.Substring (i + 1, x - i));
                    raw = raw.Substring (x + 2).Trim ();
                    i = 0;
                    break;
                case ' ':
                    x = raw.Substring (i + 1).Trim ().IndexOf (' ');
                    if (x == -1) {
                        ret.parameters.Add (raw.Substring (i + 1).Trim ());
                        i = raw.Length; //break,break!
                    } else {
                        ret.parameters.Add (raw.Substring (i + 1, x - i));
                        raw = raw.Substring (x + 1).Trim ();
                        i = 0;
                    }
                    break;
                }
            }
            return ret;
        }
    }
}