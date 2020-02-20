using System;
using System.Collections.Generic;
using System.IO;
using cqhttp.Cyan.Clients;
using cqhttp.Cyan.Events.CQEvents.Base;
using cqhttp.Cyan.Events.CQResponses;
using Newtonsoft.Json.Linq;

namespace CyanBot {
    class Program {
        public static CQApiClient client;
        public static Dictionary<string, string> Globals =
            new Dictionary<string, string> ();
        static void LoadCfg () {
            if (File.Exists ("config.json")) {
                string config = File.ReadAllText ("config.json");
                try {
                    var config_parsed = JToken.Parse (config);
                    var coolq_config = config_parsed["coolq_config"];
                    Globals = config_parsed["globals"].ToObject<Dictionary<string, string>> ();
                    client = new CQHTTPClient (
                        access_url: coolq_config["api_addr"].ToObject<string> (),
                        listen_port: coolq_config["listen_port"].ToObject<int> (),
                        access_token: coolq_config["access_token"].ToObject<string> (),
                        secret: coolq_config["listen_secret"].ToObject<string> (),
                        use_group_table: true
                    );
                } catch {
                    throw new Exception ("Config file parse error");
                }
                return;
            }
            throw new ArgumentNullException ("缺乏正常启动所需的配置文件或参数");
        }
        static void Main (string[] args) {
            LoadCfg ();
            client.OnEventAsync += async (cli, e) => {
                if (e is MessageEvent) {
                    await Dispatcher.Dispatcher.Dispatch (
                        cli, e as MessageEvent
                    );
                }
                return new EmptyResponse ();
            };
            Console.ReadLine ();
        }
    }
}