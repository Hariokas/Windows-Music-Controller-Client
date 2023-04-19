using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace Android___MusicController;

public class MainPageViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    private readonly MediaSessionManager _mediaSessionManager;

    private ImageSource _albumArtSource = "banana_1000x1000.png";

    private string _artistLabelText = "Artist Label";

    private string _idLabelText = "ID Label";

    private string _mediaSessionLabelText = "Media Session Label";

    private string _playbackStatusLabelText = "Playback Status Label";

    private string _songNameLabelText = "Song Name Label";

    private readonly WebSocketService _webSocketService;

    private int hashOfLastMessage;
    private int hashOfLastImage;

    public MainPageViewModel(WebSocketService webSocketService)
    {
        _webSocketService = webSocketService;

        _mediaSessionManager = MediaSessionManager.Instance;

        _webSocketService.OnTextMessageReceived += WebSocketService_OnTextMessageReceived;
        _webSocketService.OnBinaryMessageReceived += WebSocketService_OnBinaryMessageReceived;

        _mediaSessionManager.PropertyChanged += MediaSessionManager_PropertyChanged;

    }

    #region Properties
    public string IdLabelText
    {
        get => _idLabelText;
        set
        {
            _idLabelText = value;
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

    private void MediaSessionManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MediaSessionManager.MediaSessionInfos))
        {
            var activeMediaSession = _mediaSessionManager.GetActiveMediaSession();
            if (activeMediaSession != null) UpdateUserInterface(activeMediaSession);
        }
    }

    private void WebSocketService_OnTextMessageReceived(object sender, string message)
    {
        if (message.GetHashCode() == hashOfLastMessage) return;
        ProcessTextMessage(message);
        hashOfLastMessage = message.GetHashCode();
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

    private void ProcessTextMessage(string message)
    {
        Debug.WriteLine($"Received message: {message}");
        try
        {
            var mediaSessionEvent = JsonConvert.DeserializeObject<MediaSessionEvent>(message);
            _mediaSessionManager.HandleMediaSessionEvent(mediaSessionEvent);
        }
        catch
        {
            Debug.WriteLine($"Unknown message: {message}");
        }
    }

    private void UpdateUserInterface(MediaInfo mediaSession)
    {
        if (mediaSession == null) return;

        IdLabelText = mediaSession.PlayerId.ToString();
        MediaSessionLabelText = mediaSession.PlayerName;
        ArtistLabelText = mediaSession.Artist;
        SongNameLabelText = mediaSession.SongName;
        PlaybackStatusLabelText = mediaSession.PlaybackStatus;
    }

    private void LoadImageFromByteArray(byte[] imageDate)
    {
        Debug.WriteLine($"Received image of size: {imageDate.Length}");

        var imageSource = ImageSource.FromStream(() => new MemoryStream(imageDate));

        AlbumArtSource = imageSource;
    }

    public async void PlayPauseButtonClickedAsync()
    {
        var mediaSessionEvent = new MediaSessionEvent { EventType = EventType.Play };
        var message = JsonConvert.SerializeObject(mediaSessionEvent);
        await _webSocketService.SendMessageAsync(message);
    }

    public async void PreviousButtonClickedAsync()
    {
        var mediaSessionEvent = new MediaSessionEvent { EventType = EventType.Previous };
        var message = JsonConvert.SerializeObject(mediaSessionEvent);
        await _webSocketService.SendMessageAsync(message);
    }

    public async void NextButtonClickedAsync()
    {
        var mediaSessionEvent = new MediaSessionEvent { EventType = EventType.Next };
        var message = JsonConvert.SerializeObject(mediaSessionEvent);
        await _webSocketService.SendMessageAsync(message);
    }
}