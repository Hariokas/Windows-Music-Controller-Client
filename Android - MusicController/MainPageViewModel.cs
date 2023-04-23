using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Android___MusicController.EventClasses;
using Android___MusicController.Handlers;
using Android___MusicController.Models;
using Newtonsoft.Json;

namespace Android___MusicController;

public class MainPageViewModel : INotifyPropertyChanged
{
    private readonly MediaSessionHandler _mediaSessionHandler;
    private readonly WebSocketHandler _webSocketHandler;

    private ImageSource _albumArtSource = "banana_1000x1000.png";

    private string _playbackStatusLabel;
    private string _playbackInfoLabel;
    private string _artistLabelText = "Artist Label";
    private string _idLabelText = "ID Label";
    private string _mediaSessionLabelText = "Media Session Label";
    private string _playbackStatusLabelText = "Playback Status Label";
    private string _songNameLabelText = "Song Name Label";

    private int hashOfLastImage;
    private int hashOfLastMessage;

    public MainPageViewModel(WebSocketHandler webSocketHandler, MediaSessionHandler mediaSessionHandler)
    {
        _webSocketHandler = webSocketHandler;
        _mediaSessionHandler = mediaSessionHandler;

        _webSocketHandler.MasterVolumeInfoReceived += MasterVolume_InfoReceived;
        _webSocketHandler.MediaSessionInfoReceived += MediaSession_InfoReceived;
        _webSocketHandler.VolumeMixerInfoReceived += VolumeMixer_InfoReceived;
        _webSocketHandler.OnBinaryMessageReceived += WebSocketService_OnBinaryMessageReceived;

        _mediaSessionHandler.PropertyChanged += MediaSessionHandlerPropertyChanged;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void VolumeMixer_InfoReceived(object sender, VolumeMixerEventArgs e)
    {
        Trace.WriteLine("VolumeMixer_InfoReceived");
    }

    private void MediaSession_InfoReceived(object sender, MediaSessionEventArgs e)
    {
        if (e.GetHashCode() == hashOfLastMessage) return;
        _mediaSessionHandler.HandleMediaSessionEvent(e.MediaSessionEvent);
        hashOfLastMessage = e.GetHashCode();
    }

    private void MasterVolume_InfoReceived(object sender, MasterVolumeEventArgs e)
    {
        Trace.WriteLine("MasterVolume_InfoReceived");
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

        //IdLabelText = mediaSession.PlayerId.ToString();
        //MediaSessionLabelText = mediaSession.PlayerName;
        ArtistLabelText = mediaSession.Artist;
        SongNameLabelText = mediaSession.SongName;
        PlaybackStatusLabelText = mediaSession.PlaybackStatus;
        PlaybackStatusLabel = $"{mediaSession.PlaybackStatus} on {mediaSession.PlayerName}";
        PlaybackInfoLabel = $"[{mediaSession.Artist}] - {mediaSession.SongName}";
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

    #region Properties

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