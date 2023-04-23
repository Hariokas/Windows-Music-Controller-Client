using System.ComponentModel;
using Android___MusicController.Controls;
using Android___MusicController.Handlers;

namespace Android___MusicController;

public partial class MainPage : ContentPage
{
    private readonly MediaSessionHandler _mediaSessionHandler;
    private readonly MainPageViewModel _viewModel;
    private readonly WebSocketHandler _webSocketHandler;
    private string _lastPlaybackStatus;

    public MainPage()
    {
        InitializeComponent();
        _webSocketHandler = new WebSocketHandler("192.168.0.107", "8080");
        _mediaSessionHandler = new MediaSessionHandler();

        _mediaSessionHandler.PropertyChanged += MediaSessionHandlerPropertyChanged;
        PlaybackInfoLabelCombined.SizeChanged += OnPlaybackInfoLabelSizeChanged;

        _viewModel = new MainPageViewModel(_webSocketHandler, _mediaSessionHandler);
        BindingContext = _viewModel;
    }


    private async void OnPlaybackInfoLabelSizeChanged(object sender, EventArgs e)
    {
        await AnimateMarqueeLabel(PlaybackInfoLabelCombined);
    }

    private async void MediaSessionHandlerPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(MediaSessionHandler.MediaSessionInfos)) return;
        UpdatePlayPauseButton();
    }

    private async void UpdatePlayPauseButton()
    {
        var playbackStatus = _mediaSessionHandler.GetActiveMediaSession().PlaybackStatus;
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

    private async Task AnimateMarqueeLabel(MarqueeLabel label)
    {
        var gridWidth = ((VisualElement)label.Parent).Bounds.Width;
        var widthDifference = label.Bounds.Width - gridWidth;

        if (widthDifference <= 0) return;

        label.TranslationX = gridWidth;

        while (true)
        {
            await label.TranslateTo(-widthDifference, 0, (uint)(label.Bounds.Width * 10), Easing.Linear);
            await Task.Delay(2000);
            await label.TranslateTo(0, 0, (uint)(label.Bounds.Width * 10), Easing.Linear);
            await Task.Delay(2000);
        }
    }
}