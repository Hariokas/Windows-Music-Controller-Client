using System.Diagnostics;
using Media_Controller_Remote_Client.EventClasses;
using Newtonsoft.Json;

namespace Media_Controller_Remote_Client.HamburgerMenuPages;

public partial class MasterVolumePage : ContentPage
{
    private readonly WebSocketHandler _webSocketHandler;
    //private readonly MasterVolumeViewModel _viewModel;

    public MasterVolumePage()
    {
        InitializeComponent();
        _webSocketHandler = WebSocketHandler.Instance;
        _webSocketHandler.MasterVolumeInfoReceived += MasterVolumeInfoReceived;
        SoundLevelSlider.ValueChanged += SoundLevelSlider_ValueChanged;
    }

    private async void SoundLevelSlider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        var masterVolumeChangedEvent = new MasterVolumeEvent()
        {
            MasterVolumeEventType = MasterVolumeEventType.SetMasterVolume,
            MasterVolume = (float)SoundLevelSlider.Value
        };

        var message = JsonConvert.SerializeObject(masterVolumeChangedEvent);
        await _webSocketHandler.SendMessageAsync(message);
    }

    private void MasterVolumeInfoReceived(object sender, MasterVolumeEventArgs e)
    {
        try
        {
            switch (e.MasterVolumeEvent.MasterVolumeEventType)
            {
                case MasterVolumeEventType.GetIsMuted:
                    UpdateMuteStatus(e.MasterVolumeEvent.IsMuted);
                    break;

                case MasterVolumeEventType.GetMasterVolume:
                    UpdateVolumeSlider(e.MasterVolumeEvent.MasterVolume);
                    break;

                default:
                    Trace.WriteLine($"Unknown event type: {e.MasterVolumeEvent.MasterVolumeEventType}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex.Message);
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await SendGetMasterVolumeRequestAsync();
    }

    private async void UpdateVolumeSlider(float sliderValue)
    {
        if (sliderValue is < 0 or > 1) { return; }

        SoundLevelSlider.Value = sliderValue;
    }

    private async void UpdateMuteStatus(bool isMuted)
    {
        // do nothing
    }

    private async Task SendGetMasterVolumeRequestAsync()
    {
        var masterVolumeEvent = new MasterVolumeEvent
        {
            MasterVolumeEventType = MasterVolumeEventType.GetMasterVolume
        };

        var message = JsonConvert.SerializeObject(masterVolumeEvent);
        await _webSocketHandler.SendMessageAsync(message);
    }

}