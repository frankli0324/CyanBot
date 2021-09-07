using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cqhttp.Cyan.Enums;
using cqhttp.Cyan.Events.CQEvents.Base;
using cqhttp.Cyan.Messages;
using cqhttp.Cyan.Messages.CQElements;
using CyanBot.Modules.CTFHubUtils;


namespace CyanBot.Modules {
    public class CTFHub : Module {
        static Dictionary<(MessageType, long), IEnumerable<API.Event>> last_list =
            new Dictionary<(MessageType, long), IEnumerable<API.Event>> ();
        static Dictionary<(MessageType, long), API.Event> last_event =
            new Dictionary<(MessageType, long), API.Event> ();
        static Dictionary<(MessageType, long), int> last_page =
            new Dictionary<(MessageType, long), int> ();
        async Task<string> BuildDetail (API.Event feed, int page = 1) {
            await Task.Run (() => { });
            if (feed.description.Length <= 400) {
                return $@"[{feed.title}]
比赛地址：{feed.link}
比赛时间：{feed.start_time}

{feed.description}
———————————
比赛信息来自CTFHub(https://www.ctfhub.com/#/calendar)";
            }
            var remain = feed.description.Length % 400;
            var page_total = (feed.description.Length + 400) / 400;
            var len = Math.Min (page * 400, feed.description.Length) - (page - 1) * 400;
            var desc = feed.description.Substring ((page - 1) * 400, len);
            return $@"[{feed.title}]
比赛地址：{feed.link}
比赛时间：{feed.start_time}

{desc}
———————————
当前第 {page} 页，共 {page_total} 页
/ctf next_page 或 /ctf np 翻页
———————————
比赛信息来自CTFHub(https://www.ctfhub.com/#/calendar)";
        }
        async Task<string> BuildList (string title, IEnumerable<API.Event> list) {
            await Task.Run (() => { });
            var blist = list.ToList ();
            string msg = $"{title}的{blist.Count}场CTF比赛：\n";
            for (var i = 0; i < list.Count (); i++) {
                msg += $"{i + 1}. [{blist[i].start_time.ToString ("yyyy-MM-dd")}] {blist[i].title}\n";
            }
            msg += @"———————————
标记为*的比赛正在进行中
发送 ""/ctf info [序号]"" 以查看比赛链接
发送 ""/ctf show [序号]"" 以查看详细信息
———————————
比赛信息来自CTFHub
https://www.ctfhub.com/#/calendar";
            return msg;
        }
        [OnCommand ("ctf")]
        public async Task<Message> Expand (string[] parameters, MessageEvent e) {
            if (parameters.Length == 0)
                return new Message ("/ctf <list/upcoming/running>");
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
            case "show": {
                    int index;
                    if (!last_list.ContainsKey (e.GetEndpoint ()))
                        return new Message ("请先进行<list | upcoming | running>操作之一，如/ctf list");
                    if (!int.TryParse (parameters[1], out index) ||
                        (index < 1 || index > last_list[e.GetEndpoint ()].Count ()))
                        return new Message ();
                    var detail = await API.GetEventDetail (
                        last_list[e.GetEndpoint ()].ElementAt (index - 1).id
                    );
                    last_event[e.GetEndpoint ()] = detail;
                    last_page[e.GetEndpoint ()] = 1;
                    return new Message (await BuildDetail (detail));
                }
            case "np":
            case "next_page":
                if (!last_event.ContainsKey (e.GetEndpoint ()))
                    return new Message ("请先进行<show>操作，如/ctf show 1");
                var eo = last_event[e.GetEndpoint ()];
                try {
                    var page = last_page[e.GetEndpoint ()];
                    var msg = new Message (await BuildDetail (eo, page + 1));
                    last_page[e.GetEndpoint ()] = page + 1;
                    return msg;
                } catch (Exception) { return new Message (); }
            case "info": {
                    if (!last_list.ContainsKey (e.GetEndpoint ()))
                        return new Message ("请先进行<list | upcoming | running>操作之一，如/ctf list");
                    if (!int.TryParse (parameters[1], out var index) ||
                        (index < 1 || index > last_list[e.GetEndpoint ()].Count ()))
                        return new Message ();
                    var feed = await API.GetEventDetail (
                        last_list[e.GetEndpoint ()].ElementAt (index - 1).id
                    );
                    return new Message ($@"[{feed.title}]
比赛地址：{feed.link}
比赛时间：{feed.start_time}
———————————
比赛信息来自CTFHub(https://www.ctfhub.com/#/calendar)");
                }
            case "help":
                return new Message (new ElementImage ("https://files.frankli.site/ctfhelp.png"));
            }
            return new Message ();
        }
    }
}
