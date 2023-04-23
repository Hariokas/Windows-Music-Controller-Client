using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Android___MusicController.EventClasses
{

    public enum MediaSessionEventType
    {
        NewSession,
        CloseSession,
        SongChanged,
        PlaybackStatusChanged,
        SessionFocusChanged,
        Play,
        Pause,
        Previous,
        Next
    }

    public class MediaSessionEvent
    {
        public MediaSessionEventType MediaSessionEventType { get; set; }
        public int MediaSessionId { get; set; }
        public string MediaSessionName { get; set; }
        public string Artist { get; set; }
        public string SongName { get; set; }
        public string PlaybackStatus { get; set; }
    }
}
