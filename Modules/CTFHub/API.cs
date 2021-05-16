using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CyanBot.Modules.CTFHubUtils {
    class API {
        class Params { }
        class LimitParams : Params {
            public int limit { get; set; }
            public int offset { get; set; }
        }
        class EventInfoParams : Params {
            public int event_id { get; set; }
        }
        public class Event {
            public int id { get; set; }
            public string link { get; set; }
            public string title { get; set; }
            public string type { get; set; }
            public DateTime start_time { get; set; }
            public DateTime end_time { get; set; }
            public string description { get; set; }
        }
        class APIException : Exception { public APIException (string msg) : base (msg) { } }
        static string base_url = "https://api.ctfhub.com/User_API";
        static HttpClient client = new HttpClient ();
        static async Task<JToken> Request (string endpoint, Params p) {
            try {
                var resp = await client.PostAsync (
                    base_url + endpoint,
                    JsonContent.Create (p, p.GetType ())
                );
                var json = JToken.Parse (await resp.Content.ReadAsStringAsync ());
                if (json["status"].ToObject<bool> () != true)
                    throw new APIException (json["msg"].ToObject<string> ());
                return json["data"];
            } catch (HttpRequestException) {
                throw new APIException ("HTTP 请求失败");
            } catch (Newtonsoft.Json.JsonException) {
                throw new APIException ("JSON 解析异常");
            }
        }
        static async Task<IEnumerable<Event>> GetEvents (string endpoint) {
            var data = await Request (endpoint, new LimitParams { limit = 5, offset = 0 });
            var epoch = DateTime.UnixEpoch;
            return data["items"].Select (item => {
                return new Event {
                    id = item["id"].ToObject<int> (),
                    title = item["title"].ToObject<string> (),
                    start_time = epoch.AddSeconds (item["start_time"].ToObject<int> ()),
                    end_time = epoch.AddSeconds (item["end_time"].ToObject<int> ()),
                };
            });
        }
        public static async Task<IEnumerable<Event>> GetRunningEvents () {
            return await GetEvents ("/Event/getRunning");
        }
        public static async Task<IEnumerable<Event>> GetUpcomingEvents () {
            return await GetEvents ("/Event/getUpcoming");
        }
        public static async Task<IEnumerable<Event>> GetEvents () {
            var running = await GetRunningEvents ();
            var upcoming = await GetUpcomingEvents ();
            List<Event> result = new List<Event> ();
            foreach (var i in running) {
                i.title = '*' + i.title;
                result.Add (i);
            }
            foreach (var i in upcoming)
                result.Add (i);
            return result;
        }
        public static async Task<Event> GetEventDetail (int event_id) {
            var info = await Request ("/Event/getInfo", new EventInfoParams { event_id = event_id });
            var epoch = DateTime.UnixEpoch;
            return new Event {
                id = event_id,
                title = info["title"].ToObject<string> (),
                type = info["class"].ToObject<string> (),
                link = info["official_url"].ToObject<string> (),
                description = info["details"].ToObject<string> (),
                start_time = epoch.AddSeconds (info["start_time"].ToObject<int> ()),
                end_time = epoch.AddSeconds (info["end_time"].ToObject<int> ()),
            };
        }
    }
}