namespace Android___MusicController.EventClasses;

public class MediaSessionEventArgs : EventArgs
{
    public MediaSessionEventArgs(MediaSessionEvent mediaSessionEvent)
    {
        MediaSessionEvent = mediaSessionEvent;
    }

    public MediaSessionEvent MediaSessionEvent { get; }
}