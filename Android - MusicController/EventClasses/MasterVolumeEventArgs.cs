namespace Android___MusicController.EventClasses;

public class MasterVolumeEventArgs : EventArgs
{
    public MasterVolumeEventArgs(MasterVolumeEvent masterVolumeEvent)
    {
        MasterVolumeEvent = masterVolumeEvent;
    }

    public MasterVolumeEvent MasterVolumeEvent { get; }
}