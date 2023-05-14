using Media_Controller_Remote_Client.Models;

namespace Media_Controller_Remote_Client.EventClasses;

public enum VolumeMixerEventType
{
    GetApplicationVolumes,
    SetApplicationVolume
}

public class VolumeMixerEvent : BaseEvent
{
    public VolumeMixerEvent()
    {
        EventType = BaseEventType.VolumeMixerEvent;
    }

    public VolumeMixerEventType VolumeMixerEventType { get; set; }

    public List<ApplicationVolume> ApplicationVolumes { get; set; }

    public ApplicationVolume ApplicationVolume { get; set; }
}