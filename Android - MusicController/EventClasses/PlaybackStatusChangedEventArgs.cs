namespace Android___MusicController.EventClasses;

public class PlaybackStatusChangedEventArgs : EventArgs
{
    public PlaybackStatusChangedEventArgs(string playbackStatus)
    {
        PlaybackStatus = playbackStatus;
    }

    public string PlaybackStatus { get; }
}