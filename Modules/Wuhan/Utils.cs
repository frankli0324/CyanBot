using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace CyanBot.Modules.WuhanUtils {
    class Area : IEnumerable<Area> {
        JToken data;
        Dictionary<string, Area> children = new Dictionary<string, Area> ();
        public string name { get { return data["name"].ToObject<string> (); } }
        public JToken today { get { return data["today"]; } }
        public JToken total { get { return data["total"]; } }
        public Area (JToken d) {
            data = d;
            foreach (var i in data["children"])
                children[i["name"].ToObject<string> ()] = new Area (i);
        }
        public bool ContainsKey (string area) => children.ContainsKey (area);
        public Area this[string subarea] {
            get {
                if (children.ContainsKey (subarea))
                    return children[subarea];
                return null;
            }
        }
        public IEnumerator<Area> GetEnumerator () {
            foreach (var i in children.Values)
                yield return i;
        }
        IEnumerator IEnumerable.GetEnumerator () {
            return GetEnumerator ();
        }
    }
    static class Extensions {
        public static int confirmed (this JToken o) {
            try {
                return o["confirm"].ToObject<int> ();
            } catch { return 0; }
        }
        public static int suspected (this JToken o) {
            try {
                return o["suspect"].ToObject<int> ();
            } catch { return 0; }
        }
        public static int healed (this JToken o) {
            try {
                return o["heal"].ToObject<int> ();
            } catch { return 0; }
        }
        public static int dead (this JToken o) {
            try {
                return o["dead"].ToObject<int> ();
            } catch { return 0; }
        }
    }
}