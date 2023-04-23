using System.Diagnostics;
using Android___MusicController.EventClasses;
using Newtonsoft.Json;

namespace Android___MusicController.HamburgerMenuPages;

public partial class MasterVolumePage : ContentPage
{
    private readonly WebSocketHandler _webSocketHandler;
    //private readonly MasterVolumeViewModel _viewModel;

    public MasterVolumePage()
    {
        InitializeComponent();
        _webSocketHandler = WebSocketHandler.Instance;
        //_webSocketHandler.MasterVolumeInfoReceived += MasterVolumeInfoReceived;
        //_viewModel = new MasterVolumeViewModel();
        //BindingContext = _viewModel;
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

    //private void MasterVolumeInfoReceived(object sender, MasterVolumeEventArgs e)
    //{
    //    try
    //    {
    //        switch (e.MasterVolumeEvent.MasterVolumeEventType)
    //        {
    //            case MasterVolumeEventType.GetMasterVolume:
    //                _viewModel.SoundLevel = e.MasterVolumeEvent.MasterVolume;
    //                _viewModel.IsMuted = e.MasterVolumeEvent.IsMuted;
    //                break;

    //            case MasterVolumeEventType.SetMasterVolume:
    //                _viewModel.SoundLevel = e.MasterVolumeEvent.MasterVolume;
    //                break;

    //            case MasterVolumeEventType.GetIsMuted:
    //                _viewModel.IsMuted = e.MasterVolumeEvent.IsMuted;
    //                break;

    //            case MasterVolumeEventType.SetMute:
    //                _viewModel.IsMuted = e.MasterVolumeEvent.IsMuted;
    //                break;

    //            default:
    //                Trace.WriteLine($"Unknown event type: {e.MasterVolumeEvent.MasterVolumeEventType}");
    //                break;
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        Trace.WriteLine(ex.Message);
    //    }
    //}

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await SendGetMasterVolumeRequestAsync();
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