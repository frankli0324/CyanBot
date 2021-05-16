using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cqhttp.Cyan.Enums;
using cqhttp.Cyan.Events.CQEvents.Base;
using cqhttp.Cyan.Messages;
using CyanBot.Modules.CTFHubUtils;


namespace CyanBot.Modules {
    public class CTFHub : Module {
        static Dictionary<(MessageType, long), IEnumerable<API.Event>> last_list =
            new Dictionary<(MessageType, long), IEnumerable<API.Event>> ();
        async Task<string> BuildDetail (API.Event feed) {
            return $@"[{feed.title}]
比赛地址：{feed.link}
比赛时间：{feed.start_time}

{feed.description}
———————————
比赛信息来自CTFHub(https://www.ctfhub.com/#/calendar)";
        }
        async Task<string> BuildList (string title, IEnumerable<API.Event> list) {
            string msg = $"{title}的{list.Count ()}场CTF比赛：\n";
            foreach (var i in list) {
                msg += $"[{i.start_time.ToShortDateString ()}] {i.title}\n";
            }
            msg += @"———————————
比赛信息来自CTFHub
https://www.ctfhub.com/#/calendar";
            return msg;
        }
        [OnCommand ("ctf")]
        public async Task<Message> Expand (string[] parameters, MessageEvent e) {
            string ctrl = parameters[0];
            switch (ctrl) {
            case "next":
                return new Message (await BuildDetail (await API.GetEventDetail (
                    (await API.GetRunningEvents ()).First ().id
                )));
            case "running":
                last_list[e.GetEndpoint ()] = await API.GetRunningEvents ();
                return new Message (await BuildList ("正在进行", last_list[e.GetEndpoint ()]));
            case "upcoming":
                last_list[e.GetEndpoint ()] = await API.GetUpcomingEvents ();
                return new Message (await BuildList ("即将开始", last_list[e.GetEndpoint ()]));
            case "list":
                last_list[e.GetEndpoint ()] = await API.GetEvents ();
                return new Message (await BuildList ("接下来", last_list[e.GetEndpoint ()]));
            case "show":
                int index;
                if (!last_list.ContainsKey (e.GetEndpoint ()))
                    return new Message ("请先进行<list | upcoming | running>操作之一，如/ctf list");
                if (!int.TryParse (parameters[1], out index) ||
                    (index < 1 || index > last_list[e.GetEndpoint ()].Count ()))
                    return new Message ();
                return new Message (await BuildDetail (await API.GetEventDetail (
                    last_list[e.GetEndpoint ()].ElementAt (index - 1).id
                )));
            }
            return new Message ();
        }
    }
}
