namespace Media_Controller_Remote_Client.EventClasses;

public enum BaseEventType
{
    MasterVolumeEvent,
    MediaSessionEvent,
    VolumeMixerEvent
}

public class BaseEvent
{
    public BaseEventType EventType { get; set; }
}