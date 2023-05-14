namespace Media_Controller_Remote_Client.EventClasses;

public class MediaSessionEventArgs : EventArgs
{
    public MediaSessionEventArgs(MediaSessionEvent mediaSessionEvent)
    {
        MediaSessionEvent = mediaSessionEvent;
    }

    public MediaSessionEvent MediaSessionEvent { get; }
}