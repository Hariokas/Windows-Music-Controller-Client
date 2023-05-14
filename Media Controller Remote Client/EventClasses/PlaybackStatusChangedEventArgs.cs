namespace Media_Controller_Remote_Client.EventClasses;

public class PlaybackStatusChangedEventArgs : EventArgs
{
    public PlaybackStatusChangedEventArgs(string playbackStatus)
    {
        PlaybackStatus = playbackStatus;
    }

    public string PlaybackStatus { get; }
}