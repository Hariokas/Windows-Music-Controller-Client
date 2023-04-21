using System.Diagnostics;
using Newtonsoft.Json;

namespace Android___MusicController;

public partial class MainPage : ContentPage
{
    private readonly WebSocketService _webSocketService;
    private readonly MainPageViewModel _viewModel;
    private readonly MediaSessionManager _sessionManager;
    private string _lastPlaybackStatus;

    public MainPage()
    {
        InitializeComponent();
        _webSocketService = new WebSocketService("192.168.0.107", "8080");
        _sessionManager = MediaSessionManager.Instance;

        _sessionManager.PropertyChanged += _sessionManager_PropertyChanged;

        _viewModel = new MainPageViewModel(_webSocketService);
        this.BindingContext = _viewModel;
    }

    private async void _sessionManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(MediaSessionManager.MediaSessionInfos)) return;
        UpdatePlayPauseButton();
    }

    private async void UpdatePlayPauseButton()
    {
        var playbackStatus = _sessionManager.GetActiveMediaSession().PlaybackStatus;
        if (_lastPlaybackStatus == playbackStatus) return;

        _lastPlaybackStatus = playbackStatus;

        switch (playbackStatus)
        {
            case "Playing":
                PlayPauseButton.Source = "pause.png";
                break;
            default:
                PlayPauseButton.Source = "play.png";
                break;
        }
    }

    private async void PreviousButton_OnClicked(object sender, EventArgs e)
    {
        _viewModel.PreviousButtonClickedAsync();
    }

    private async void PlayPauseButton_OnClicked(object sender, EventArgs e)
    {
        _viewModel.PlayPauseButtonClickedAsync();
    }

    private async void NextButton_OnClicked(object sender, EventArgs e)
    {
        _viewModel.NextButtonClickedAsync();
    }
}