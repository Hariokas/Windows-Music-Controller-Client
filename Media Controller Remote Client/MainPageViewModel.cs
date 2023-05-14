using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Media_Controller_Remote_Client.Controllers;
using Media_Controller_Remote_Client.EventClasses;
using Media_Controller_Remote_Client.Handlers;
using Media_Controller_Remote_Client.Models;
using Newtonsoft.Json;

namespace Media_Controller_Remote_Client;

public class MainPageViewModel : INotifyPropertyChanged
{
    private readonly MasterVolumeController _masterVolumeController;
    private readonly MediaSessionHandler _mediaSessionHandler;
    private readonly WebSocketHandler _webSocketHandler;

    private ImageSource _albumArtSource = "banana_1000x1000.png";
    private string _artistLabelText = "Artist Label";
    private string _idLabelText = "ID Label";

    private bool _isMuted;
    private float _masterSoundLevel = 1;
    private string _mediaSessionLabelText = "Media Session Label";
    private string _playbackInfoLabel;
    private TimeSpan _playbackPosition;
    private double _playbackProgress;

    private string _playbackStatusLabel = "Waiting for your music to start";
    private string _playbackStatusLabelText = "Playback Status Label";

    private CancellationTokenSource _playbackTimerCancellationTokenSource = new();
    private TimeSpan _songDuration;
    private string _songNameLabelText = "Song Name Label";

    private int hashOfLastImage;
    private int hashOfLastMessage;

    private bool _timerRunning;

    public MainPageViewModel(WebSocketHandler webSocketHandler, MediaSessionHandler mediaSessionHandler)
    {
        _webSocketHandler = webSocketHandler;
        _mediaSessionHandler = mediaSessionHandler;
        _masterVolumeController = MasterVolumeController.Instance;

        _webSocketHandler.MediaSessionInfoReceived += MediaSession_InfoReceived;
        _webSocketHandler.VolumeMixerInfoReceived += VolumeMixer_InfoReceived;
        _webSocketHandler.OnBinaryMessageReceived += WebSocketService_OnBinaryMessageReceived;

        _mediaSessionHandler.PropertyChanged += MediaSessionHandlerPropertyChanged;
        _mediaSessionHandler.PlaybackStatusChanged += MediaSessionHandler_PlaybackStatusChanged;

        _masterVolumeController.PropertyChanged += MasterVolumeController_PropertyChanged;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private async void MediaSessionHandler_PlaybackStatusChanged(object sender, PlaybackStatusChangedEventArgs e)
    {
        if (e.PlaybackStatus == "Paused" && _timerRunning && !_playbackTimerCancellationTokenSource.IsCancellationRequested)
        {
            _playbackTimerCancellationTokenSource.Cancel();
        }
        else if (e.PlaybackStatus == "Playing" && !_timerRunning)
        {
            _playbackTimerCancellationTokenSource = new CancellationTokenSource();
            StartPlaybackTimer();
        }
    }

    private void MasterVolumeController_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(MasterVolumeController.IsMuted):
                OnPropertyChanged(nameof(IsMuted));
                break;
            case nameof(MasterVolumeController.SoundLevel):
                OnPropertyChanged(nameof(MasterSoundLevel));
                break;
        }
    }

    private void VolumeMixer_InfoReceived(object sender, VolumeMixerEventArgs e)
    {
        Trace.WriteLine("VolumeMixer_InfoReceived");
        var a = e.VolumeMixerEvent;
        var b = "wsup";
    }

    private void MediaSession_InfoReceived(object sender, MediaSessionEventArgs e)
    {
        if (e.GetHashCode() == hashOfLastMessage) return;
        _mediaSessionHandler.HandleMediaSessionEvent(e.MediaSessionEvent);
        hashOfLastMessage = e.GetHashCode();
    }

    private void MediaSessionHandlerPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MediaSessionHandler.MediaSessionInfos))
        {
            var activeMediaSession = _mediaSessionHandler.GetActiveMediaSession();
            if (activeMediaSession != null) UpdateUserInterface(activeMediaSession);
        }
    }

    private void WebSocketService_OnBinaryMessageReceived(object sender, byte[] imageData)
    {
        if (imageData.GetHashCode() == hashOfLastImage) return;
        LoadImageFromByteArray(imageData);
        hashOfLastImage = imageData.GetHashCode();
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void UpdateUserInterface(MediaInfo mediaSession)
    {
        if (mediaSession == null) return;

        ArtistLabelText = mediaSession.Artist;
        SongNameLabelText = mediaSession.SongName;
        PlaybackStatusLabelText = mediaSession.PlaybackStatus;
        PlaybackStatusLabel = $"{mediaSession.PlaybackStatus} on {mediaSession.PlayerName}";
        PlaybackInfoLabel = $"[{mediaSession.Artist}] - {mediaSession.SongName}";
        PlaybackPosition = mediaSession.CurrentPlaybackTime;
        SongDuration = mediaSession.SongLength;
        PlaybackProgress = mediaSession.CurrentPlaybackTime.TotalSeconds / mediaSession.SongLength.TotalSeconds;
    }

    private async void StartPlaybackTimer()
    {
        if (_timerRunning) return;

        _timerRunning = true;

        try
        {
            var token = _playbackTimerCancellationTokenSource.Token;
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), token);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    PlaybackPosition += TimeSpan.FromSeconds(1);
                    PlaybackProgress = PlaybackPosition.TotalSeconds / SongDuration.TotalSeconds;
                });
            }
        }
        catch (TaskCanceledException)
        {

        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex.Message);
        }
        finally
        {
            _timerRunning = false;
        }
    }

    private void LoadImageFromByteArray(byte[] imageDate)
    {
        Debug.WriteLine($"Received image of size: {imageDate.Length}");

        var imageSource = ImageSource.FromStream(() => new MemoryStream(imageDate));

        AlbumArtSource = imageSource;
    }

    public async void PlayPauseButtonClickedAsync()
    {
        var mediaSessionEvent = new MediaSessionEvent { MediaSessionEventType = MediaSessionEventType.Play };
        var message = JsonConvert.SerializeObject(mediaSessionEvent);
        await _webSocketHandler.SendMessageAsync(message);
    }

    public async void PreviousButtonClickedAsync()
    {
        var mediaSessionEvent = new MediaSessionEvent { MediaSessionEventType = MediaSessionEventType.Previous };
        var message = JsonConvert.SerializeObject(mediaSessionEvent);
        await _webSocketHandler.SendMessageAsync(message);
    }

    public async void NextButtonClickedAsync()
    {
        var mediaSessionEvent = new MediaSessionEvent { MediaSessionEventType = MediaSessionEventType.Next };
        var message = JsonConvert.SerializeObject(mediaSessionEvent);
        await _webSocketHandler.SendMessageAsync(message);
    }

    public async Task RequestVolumeData()
    {
        await _masterVolumeController.GetMasterVolumeData();
    }

    #region Properties

    public bool IsMuted
    {
        get => _isMuted;
        set
        {
            if (_isMuted == value) return;
            _isMuted = value;
            _masterVolumeController.IsMuted = value;
            OnPropertyChanged();
        }
    }

    public float MasterSoundLevel
    {
        get => _masterSoundLevel;
        set
        {
            if (_masterSoundLevel == value || value < 0 || value > 1) return;
            _masterSoundLevel = value;
            _masterVolumeController.SoundLevel = value;
            OnPropertyChanged();
        }
    }

    public TimeSpan SongDuration
    {
        get => _songDuration;
        set
        {
            _songDuration = value;
            OnPropertyChanged();
        }
    }

    public TimeSpan PlaybackPosition
    {
        get => _playbackPosition;
        set
        {
            _playbackPosition = value;
            OnPropertyChanged();
        }
    }

    public double PlaybackProgress
    {
        get => _playbackProgress;
        set
        {
            if (value is < 0 or > 1) return;
            _playbackProgress = value;
            OnPropertyChanged();
        }
    }

    public string PlaybackStatusLabel
    {
        get => _playbackStatusLabel;
        set
        {
            _playbackStatusLabel = value;
            OnPropertyChanged();
        }
    }

    public string PlaybackInfoLabel
    {
        get => _playbackInfoLabel;
        set
        {
            _playbackInfoLabel = value;
            OnPropertyChanged();
        }
    }

    public string ArtistLabelText
    {
        get => _artistLabelText;
        set
        {
            _artistLabelText = value;
            OnPropertyChanged();
        }
    }

    public string MediaSessionLabelText
    {
        get => _mediaSessionLabelText;
        set
        {
            _mediaSessionLabelText = value;
            OnPropertyChanged();
        }
    }

    public string PlaybackStatusLabelText
    {
        get => _playbackStatusLabelText;
        set
        {
            _playbackStatusLabelText = value;
            OnPropertyChanged();
        }
    }

    public string SongNameLabelText
    {
        get => _songNameLabelText;
        set
        {
            _songNameLabelText = value;
            OnPropertyChanged();
        }
    }

    public ImageSource AlbumArtSource
    {
        get => _albumArtSource;
        set
        {
            _albumArtSource = value;
            OnPropertyChanged();
        }
    }

    #endregion
}