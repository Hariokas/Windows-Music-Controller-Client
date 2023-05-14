namespace Media_Controller_Remote_Client.EventClasses;

public class MasterVolumeEventArgs : EventArgs
{
    public MasterVolumeEventArgs(MasterVolumeEvent masterVolumeEvent)
    {
        MasterVolumeEvent = masterVolumeEvent;
    }

    public MasterVolumeEvent MasterVolumeEvent { get; }
}