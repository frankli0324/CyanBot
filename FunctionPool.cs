using System;
using System.Collections.Generic;
using cqhttp.Cyan.Enums;
using cqhttp.Cyan.Messages;
using CyanBot.Functions;

namespace CyanBot {
    public static class FunctionPool {
        public static Dictionary<string, Func<Dispatcher.Command, Message>> onCommand =
            new Dictionary<string, Func<Dispatcher.Command, Message>> ();
        public static List < Func < (MessageType, long), Message, Message >> onAny =
            new List < Func < (MessageType, long), Message, Message >> ();
        public static void Initialize () {
            AutoReply.Register ();
            Hitokoto.Register ();
            Music.Register ();
            Repeater.Register ();
            Time.Register ();
        }
    }
}