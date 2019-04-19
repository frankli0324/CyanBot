using cqhttp.Cyan.Messages;
using cqhttp.Cyan.Messages.CQElements;

namespace CyanBot.Functions {
    public class Music {
        public static void Register () {
            FunctionPool.onCommand.Add ("listen", (p) => new Message (new ElementMusic (
                "163", p[2]
            )));
            FunctionPool.onCommand.Add ("点歌", (p) => new Message (new ElementMusic (
                "163", p[2]
            )));
        }
    }
}