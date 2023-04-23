namespace Android___MusicController.EventClasses;

public class VolumeMixerEventArgs : EventArgs
{
    public VolumeMixerEventArgs(VolumeMixerEvent volumeMixerEvent)
    {
        VolumeMixerEvent = volumeMixerEvent;
    }

    public VolumeMixerEvent VolumeMixerEvent { get; }
}