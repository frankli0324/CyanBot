using System;
using System.IO;
using cqhttp.Cyan.Clients;
using cqhttp.Cyan.Events.CQEvents.Base;
using cqhttp.Cyan.Events.CQResponses;
using Newtonsoft.Json.Linq;

namespace CyanBot {
    static class Config {
        public static string api_addr, access_token, listen_secret;
        public static int listen_port;
        public static long super_user;
    }
    class Program {
        public static CQApiClient client;

        static void LoadCfg () {
            if (File.Exists ("config.json")) {
                string config = File.ReadAllText ("config.json");

                try {
                    var config_parsed = JToken.Parse (config);
                    var coolq_config = config_parsed["coolq_config"];
                    Config.api_addr = coolq_config["api_addr"].ToObject<string> ();
                    Config.listen_port = coolq_config["listen_port"].ToObject<int> ();
                    Config.access_token = coolq_config["access_token"].ToObject<string> ();
                    Config.listen_secret = coolq_config["listen_secret"].ToObject<string> ();
                    Config.super_user = config_parsed["super_user"].ToObject<long> ();
                } catch {
                    throw new Exception ("Config file parse error");
                }
                return;
            }
            throw new ArgumentNullException ("缺乏正常启动所需的配置文件或参数");
        }
        static void Main (string[] args) {
            Dispatcher.Dispatcher.ParseCommand("/loadmodule AutoReply");
            LoadCfg ();
            client = new CQHTTPClient (
                access_url: Config.api_addr,
                listen_port: Config.listen_port,
                access_token: Config.access_token,
                secret: Config.listen_secret,
                use_group_table: true
            );
            Console.WriteLine (client.self_id);

            client.OnEvent += (cli, e) => {
                if (e is MessageEvent) {
                    Dispatcher.Dispatcher.Dispatch (
                        cli,
                        e as MessageEvent
                    );
                }
                return new EmptyResponse ();
            };
            Console.ReadLine ();
        }
    }
}