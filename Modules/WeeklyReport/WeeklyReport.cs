using System.Linq;
using System.Threading.Tasks;
using cqhttp.Cyan.Events.CQEvents.Base;
using cqhttp.Cyan.Messages;
using CyanBot.Modules.WeeklyReportUtils;

namespace CyanBot.Modules {
    class WeeklyReport : Module {

        [OnCommand ("status")]
        public async static Task<Message> Status (string[] parameters, MessageEvent e) {
            await WRScraper.updateIndex ();
            return new Message ($"{WRScraper.submittedUsers.Count}人已提交周报");
        }
        [OnCommand ("remind")]
        public async static Task<Message> Remind (string[] parameters, MessageEvent e) {
            await WRScraper.updateIndex ();
            var unsubmitted = (
                from user in WRScraper.users.Keys
                where !WRScraper.submittedUsers.Contains (user)
                select user
            ).ToList ();
            if (unsubmitted.Count > 0) {
                string toSend = $"仍然有{unsubmitted.Count}名铁憨憨没有交周报。他们分别是:\n";
                unsubmitted.ForEach ((s) => toSend += s + ',');
                toSend = toSend.TrimEnd (',');
                toSend += "\n清退警告⚠️";
                return new Message (toSend);
            }
            return new Message ("终于有一次全员交齐了");
        }
    }
}