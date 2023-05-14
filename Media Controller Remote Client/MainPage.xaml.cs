using System.ComponentModel;
using System.Diagnostics;
using Media_Controller_Remote_Client.Controls;
using Media_Controller_Remote_Client.HamburgerMenuPages;
using Media_Controller_Remote_Client.Handlers;

namespace Media_Controller_Remote_Client;

public partial class MainPage : ContentPage
{
    private readonly MediaSessionHandler _mediaSessionHandler;
    private readonly MainPageViewModel _viewModel;
    private readonly WebSocketHandler _webSocketHandler;

    private CancellationTokenSource _animationCancellationTokenSource;
    private string _lastPlaybackStatus;
    private CancellationTokenSource _soundControlGridVisibilityCancellationTokenSource;

    private bool _isPageLoaded = false;

    public MainPage()
    {
        InitializeComponent();

        SoundControlGrid.IsVisible = false;
        SoundControlGrid.ZIndex = -1;

        _webSocketHandler = WebSocketHandler.Instance;
        _mediaSessionHandler = MediaSessionHandler.Instance;

        _mediaSessionHandler.PropertyChanged += MediaSessionHandlerPropertyChanged;
        PlaybackInfoLabelCombined.SizeChanged += OnPlaybackInfoLabelSizeChanged;

        VolumeSlider.ValueChanged += VolumeSlider_ValueChanged;

        _viewModel = new MainPageViewModel(_webSocketHandler, _mediaSessionHandler);
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _isPageLoaded = true;
    }

    private void VolumeSlider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        //throw new NotImplementedException();
    }

    private async void OnPlaybackInfoLabelSizeChanged(object sender, EventArgs e)
    {
        _animationCancellationTokenSource?.Cancel();
        _animationCancellationTokenSource = new CancellationTokenSource();
        await AnimateMarqueeLabel(PlaybackInfoLabelCombined, _animationCancellationTokenSource.Token);
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
            case "Paused":
                PlayPauseButton.Source = "play.png";
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

    private async Task AnimateMarqueeLabel(MarqueeLabel label, CancellationToken cancellationToken)
    {
        try
        {
            var gridWidth = ((VisualElement)label.Parent).Bounds.Width;
            var widthDifference = label.Bounds.Width - gridWidth;

            if (widthDifference <= 0) return;

            label.TranslationX = gridWidth;

            while (true)
            {
                if (cancellationToken.IsCancellationRequested) break;
                await label.TranslateTo(-widthDifference, 0, (uint)(label.Bounds.Width * 10), Easing.Linear);

                await Task.Delay(2000, cancellationToken);

                if (cancellationToken.IsCancellationRequested) break;
                await label.TranslateTo(0, 0, (uint)(label.Bounds.Width * 10), Easing.Linear);

                await Task.Delay(2000, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex);
        }
    }

    private async Task SetSoundControlGridVisibilityTimeoutAsync()
    {
        _soundControlGridVisibilityCancellationTokenSource?.Cancel();
        _soundControlGridVisibilityCancellationTokenSource = new CancellationTokenSource();

        try
        {
            SoundControlGrid.IsVisible = true;
            SoundControlGrid.ZIndex = 10;

            await Task.Delay(TimeSpan.FromSeconds(3), _soundControlGridVisibilityCancellationTokenSource.Token);

            SoundControlGrid.IsVisible = false;
            SoundControlGrid.ZIndex = -1;
        }
        catch (TaskCanceledException)
        {
            //Task was cancelled, do nothing.
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex.Message);
        }
    }

    private async void VolumeButton_OnClicked(object sender, EventArgs e)
    {
        try
        {
            if (!_isPageLoaded) return;
            await _viewModel.RequestVolumeData();
            VolumeSlider.Value = _viewModel.MasterSoundLevel;
            _ = SetSoundControlGridVisibilityTimeoutAsync();
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex);
        }
    }

    private async void VolumeDownImageButton_OnClicked(object sender, EventArgs e)
    {
        try
        {
            if (!_isPageLoaded) return;
            _ = SetSoundControlGridVisibilityTimeoutAsync();
            var currentSoundLevel = _viewModel.MasterSoundLevel;
            var roundedSoundLevel = Math.Round(currentSoundLevel, 1);
            _viewModel.MasterSoundLevel = (float)((float)roundedSoundLevel - 0.1);
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex);
        }
    }

    private async void VolumeSlider_OnValueChanged(object sender, ValueChangedEventArgs e)
    {
        try
        {
            if (!_isPageLoaded) return;
            _ = SetSoundControlGridVisibilityTimeoutAsync();

            var currentSoundLevel = _viewModel.MasterSoundLevel;
            var roundedSoundLevel = Math.Round(currentSoundLevel, 1);

            if (Math.Round(VolumeSlider.Value, 1) == roundedSoundLevel) return;

            _viewModel.MasterSoundLevel = (float)((float)roundedSoundLevel + 0.1);
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex);
        }
    }

    private void VolumeUpImageButton_OnClicked(object sender, EventArgs e)
    {
        try
        {
            if (!_isPageLoaded) return;
            _ = SetSoundControlGridVisibilityTimeoutAsync();
            var currentSoundLevel = _viewModel.MasterSoundLevel;
            var roundedSoundLevel = Math.Round(currentSoundLevel, 1);
            _viewModel.MasterSoundLevel = (float)((float)roundedSoundLevel + 0.1);
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex);
        }
    }

    private async void PlaylistButton_OnClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SessionsPage(), true);
    }

    private async void VolumeMixerButton_OnClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new VolumeMixerPage(), true);
    }
}