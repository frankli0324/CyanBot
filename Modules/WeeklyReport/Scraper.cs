using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CyanBot.Modules.WeeklyReportUtils {

    class WRScraper {
        static Regex ContentMatcher = new Regex (@"<pre style=""white-space:pre-wrap; word-wrap:break-word;"">([\w\W]*)<\/pre>", RegexOptions.Multiline | RegexOptions.Compiled);
        static Regex TableColumnMatcher = new Regex (@"<tr>([\s\S]*?)<\/tr>", RegexOptions.Multiline | RegexOptions.Compiled);
        static Regex WeekMatcher = new Regex (@"location\.href='\/view\/week\/([0-9]*)'", RegexOptions.Multiline | RegexOptions.Compiled);
        static Regex BgcolorMatcher = new Regex (@"bgcolor='(.*?)'", RegexOptions.Multiline | RegexOptions.Compiled);
        static Regex UserMatcher = new Regex (@"onclick=""location\.href='\/view\/user\/([0-9]*)'"">(.*)<\/td>", RegexOptions.Multiline | RegexOptions.Compiled);
        public static Dictionary<string, int> users { private set; get; } = new Dictionary<string, int> ();
        public static List<string> submittedUsers { private set; get; } = new List<string> ();
        static string indexPage = "", currentWeek = "";
        static DateTime lastUpdate = new DateTime ();
        static TimeSpan updateInterval = new TimeSpan (0, 1, 0);
        static Match GetLastMatch (Match t) {
            Match ret = t;
            while (t.Success) {
                ret = t;
                t = t.NextMatch ();
            }
            return ret;
        }
        public async static Task updateIndex () {
            if (DateTime.Now - lastUpdate < updateInterval)
                return;
            using (HttpClient cli = new HttpClient ()) {
                var res = await cli.GetAsync ("http://wr.xdsec.club/index");
                indexPage = await res.Content.ReadAsStringAsync ();
            }
            Match w = WeekMatcher.Match (indexPage);
            currentWeek = GetLastMatch (w).Groups[1].Value; // 6th row
            submittedUsers.Clear ();
            Match u = TableColumnMatcher.Match (indexPage).NextMatch (); //skips the table headers
            while (u.Success) {
                Match user = UserMatcher.Match (u.Value);
                //assert(user.Success == true)
                users[user.Groups[2].Value] = int.Parse (user.Groups[1].Value);
                if (GetLastMatch (BgcolorMatcher.Match (u.Value)).Groups[1].Value == "green")
                    submittedUsers.Add (user.Groups[2].Value);
                u = u.NextMatch ();
            }
        }
        public async static Task<string> getWRFor (HttpClient cli, string username) {
            await updateIndex ();
            if (users.ContainsKey (username) == false) {
                return "用户名输入错误";
            } else if (submittedUsers.Contains (username) == false) {
                return "尚未提交";
            }
            var resp = await cli.GetAsync ($"http://wr.xdsec.club/view/user/{users[username]}/{currentWeek}");
            return ContentMatcher.Match (await resp.Content.ReadAsStringAsync ()).Groups[1].Value;
        }
        public async static Task submitWR (HttpClient cli, string wr) {
            await updateIndex ();
            await cli.PostAsync ("http://wr.xdsec.club/submit", new FormUrlEncodedContent (
                new List<KeyValuePair<string, string>> {
                    new KeyValuePair<string, string> ("content", wr),
                    new KeyValuePair<string, string> ("submi", "提交")
                }));
        }
    }
}