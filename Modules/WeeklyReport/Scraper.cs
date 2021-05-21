using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CyanBot.Modules.WeeklyReportUtils {

    class WRScraper {
        static Regex TableColumnMatcher = new Regex (@"<tr>([\s\S]*?)<\/tr>", RegexOptions.Multiline | RegexOptions.Compiled);
        static Regex WeekMatcher = new Regex (@"location\.href='\/view\/week\/([0-9]*)'", RegexOptions.Multiline | RegexOptions.Compiled);
        static Regex BgcolorMatcher = new Regex (@"bgcolor='(.*?)'", RegexOptions.Multiline | RegexOptions.Compiled);
        static Regex UserMatcher = new Regex (@"location\.href='\/view\/user\/([0-9]*)'.*?"">(.*)<\/td>", RegexOptions.Multiline | RegexOptions.Compiled);
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
            lastUpdate = DateTime.Now;
            using (HttpClient cli = new HttpClient ()) {
                var res = await cli.GetAsync ("https://wr.xdsec.org/bot");
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
    }
}