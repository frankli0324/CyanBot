using cqhttp.Cyan.Events.CQEvents.Base;
using cqhttp.Cyan.Messages;
using cqhttp.Cyan.Messages.CQElements;

namespace CyanBot.Modules {
    public class Music : Module {
        [OnCommand ("listen")]
        [OnCommand ("点歌")]
        public Message Listen (string[] parameters, MessageEvent e) =>
            new Message (new ElementMusic (
                "163", ElementMusic.GetMusicID ("163", parameters[0]).Result
            ));
    }
}