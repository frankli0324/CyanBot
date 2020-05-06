using System;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace CyanBot.Modules.CTFdUtils {

    public class CTFdClient : HttpClient {
        public string host { get; private set; }
        static Regex csrf_matcher = new Regex (
            @"\'?csrf(Nonce|_nonce)\'? ?[:=] ""(.*)""",
            RegexOptions.Compiled | RegexOptions.Multiline
        );
        public CTFdClient (string host) : base () {
            this.host = host.TrimEnd ('/');
        }
        void UpdateCSRF (string content) {
            DefaultRequestHeaders.Remove ("CSRF-Token");
            DefaultRequestHeaders.Add ("CSRF-Token",
                csrf_matcher.Match (content).Groups[2].Value
            );
        }
        public async Task Login (string username, string password) {
            UpdateCSRF (await GetStringAsync (host + "/login"));
            FormUrlEncodedContent form = new FormUrlEncodedContent (
                new Dictionary<string, string> {
                    ["name"] = username,
                    ["password"] = password,
                    ["nonce"] = DefaultRequestHeaders.GetValues (
                        "CSRF-Token"
                    ).First ()
                }
            );
            UpdateCSRF (
                await PostAsync (host + "/login", form)
                    .Result.Content.ReadAsStringAsync ()
            );
        }
        public async IAsyncEnumerable<Event> GetEvents () {
            while (true) {
                EventGenerator generator = new EventGenerator ();
                using (var stream = await GetStreamAsync (host + "/events")) {
                    while (true) {
                        int b = stream.ReadByte ();
                        if (b == -1) break;
                        var evnt = generator.ConsumeByte ((byte) b);
                        if (evnt != null) yield return evnt;
                    }
                }
                Thread.Sleep (1000);
            }
        }
    }
    public class Event {
        public string type;
        public string data;
        public Event (string e) {
            foreach (var l in e.Trim ('\n', ' ').Split ('\n')) {
                var (k, v) = l.Split (':', 2);
                if (k == "event") type = v;
                else if (k == "data") data = v;
            }
        }
    }
    class EventGenerator {
        enum State { consuming, endofline }
        State state;
        List<byte> current = new List<byte> ();
        public Event ConsumeByte (byte b) {
            if (b == '\n') {
                if (state == State.endofline) {
                    var estr = current.ToArray ();
                    current = new List<byte> ();
                    return new Event (
                        Encoding.UTF8.GetString (estr)
                    );
                }
                state = State.endofline;
            } else state = State.consuming;
            current.Add (b);
            return null;
        }
    }
}