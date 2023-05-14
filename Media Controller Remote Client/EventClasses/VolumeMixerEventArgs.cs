namespace Media_Controller_Remote_Client.EventClasses;

public class VolumeMixerEventArgs : EventArgs
{
    public VolumeMixerEventArgs(VolumeMixerEvent volumeMixerEvent)
    {
        VolumeMixerEvent = volumeMixerEvent;
    }

    public VolumeMixerEvent VolumeMixerEvent { get; }
}