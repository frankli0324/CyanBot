using System.Collections.Generic;
using cqhttp.Cyan.Enums;
using cqhttp.Cyan.Events.CQEvents.Base;
using cqhttp.Cyan.Messages;

namespace CyanBot.Modules {
    public class Repeater : Module {
        const int wait = 3;
        static Dictionary < (MessageType, long), Message > last =
            new Dictionary < (MessageType, long), Message > ();
        static Dictionary < (MessageType, long), int > repeated =
            new Dictionary < (MessageType, long), int > ();

        [OnMessage]
        Message Repeat (Message message, MessageEvent e) {
            if (last.GetValueOrDefault (e.GetEndpoint (), new Message ()) == message)
                repeated[e.GetEndpoint ()]++;
            else {
                last[e.GetEndpoint ()] = message;
                repeated[e.GetEndpoint ()] = 1;
            }
            if (repeated[e.GetEndpoint ()] == wait)
                return message;
            return new Message ();
        }
    }
}