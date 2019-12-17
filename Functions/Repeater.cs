using System;
using System.Collections.Generic;
using cqhttp.Cyan.Enums;
using cqhttp.Cyan.Messages;

namespace CyanBot.Functions {
    public class Repeater {
        const int wait = 3;
        static Dictionary < (MessageType, long), Message > last =
            new Dictionary < (MessageType, long), Message > ();
        static Dictionary < (MessageType, long), int > repeated =
            new Dictionary < (MessageType, long), int > ();
        static Message repeat ((MessageType, long) endPoint, Message message) {
            if (last.GetValueOrDefault (endPoint, new Message ()) == message)
                repeated[endPoint]++;
            else {
                last[endPoint] = message;
                repeated[endPoint] = 1;
            }
            if (repeated[endPoint] == wait)
                return message;
            return new Message ();
        }
        public static void LoadModule () => FunctionPool.onAny.Add (repeat);
        public static void UnloadModule () => FunctionPool.onAny.Remove (repeat);
    }
}