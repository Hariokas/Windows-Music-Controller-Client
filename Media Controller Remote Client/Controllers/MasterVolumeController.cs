using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Media_Controller_Remote_Client.EventClasses;
using Media_Controller_Remote_Client.Handlers;
using Newtonsoft.Json;

namespace Media_Controller_Remote_Client.Controllers;

public class MasterVolumeController : INotifyPropertyChanged
{
    private static readonly Lazy<MasterVolumeController> _lazyInstance = new(() => new MasterVolumeController());

    private readonly WebSocketHandler _webSocketHandler = WebSocketHandler.Instance;

    private TaskCompletionSource<bool> _masterVolumeInfoReceivedTcs;

    private bool? _muted = null;
    private float? _soundLevel = null;

    public MasterVolumeController()
    {
        _webSocketHandler.MasterVolumeInfoReceived += MasterVolume_InfoReceived;
    }

    public static MasterVolumeController Instance => _lazyInstance.Value;

    public float? SoundLevel
    {
        get
        {
            if (_soundLevel is null)
                GetMasterVolumeData();

            return _soundLevel;
        }
        set
        {
            if (_soundLevel == value) return;

            _soundLevel = value;
            SetMasterVolume(_soundLevel.Value);
            OnPropertyChanged();
        }
    }

    public bool? IsMuted
    {
        get
        {
            if (_muted is null)
                GetMasterVolumeData();

            return _muted;
        }
        set
        {
            if (_muted == value) return;

            _muted = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void MasterVolume_InfoReceived(object sender, MasterVolumeEventArgs e)
    {
        try
        {
            Trace.WriteLine("MasterVolume_InfoReceived");
            switch (e.MasterVolumeEvent.MasterVolumeEventType)
            {
                case MasterVolumeEventType.GetMasterVolume:
                    SoundLevel = e.MasterVolumeEvent.MasterVolume;
                    IsMuted = e.MasterVolumeEvent.IsMuted;
                    break;

                case MasterVolumeEventType.SetMasterVolume:
                    SoundLevel = e.MasterVolumeEvent.MasterVolume;
                    break;

                case MasterVolumeEventType.GetIsMuted:
                    IsMuted = e.MasterVolumeEvent.IsMuted;
                    break;

                case MasterVolumeEventType.SetMute:
                    IsMuted = e.MasterVolumeEvent.IsMuted;
                    break;

                default:
                    Trace.WriteLine($"Unknown event type: {e.MasterVolumeEvent.MasterVolumeEventType}");
                    break;
            }

            _masterVolumeInfoReceivedTcs?.TrySetResult(true);
        }
        catch (Exception ex)
        {
            _masterVolumeInfoReceivedTcs?.TrySetException(ex);
        }
    }

    public async void SetMasterVolume(float volumeLevel)
    {
        if (volumeLevel is < 0 or > 1) return;
        var masterVolumeChangedEvent = new MasterVolumeEvent
        {
            MasterVolumeEventType = MasterVolumeEventType.SetMasterVolume,
            MasterVolume = volumeLevel
        };

        var message = JsonConvert.SerializeObject(masterVolumeChangedEvent);
        await _webSocketHandler.SendMessageAsync(message);
    }

    public async Task GetMasterVolumeData()
    {
        var masterVolumeEvent = new MasterVolumeEvent
        {
            MasterVolumeEventType = MasterVolumeEventType.GetMasterVolume
        };

        var message = JsonConvert.SerializeObject(masterVolumeEvent);
        await _webSocketHandler.SendMessageAsync(message);

        _masterVolumeInfoReceivedTcs = new TaskCompletionSource<bool>();
        await _masterVolumeInfoReceivedTcs.Task;
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}