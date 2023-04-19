using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Android___MusicController;

public class MediaSessionManager
{
    private static readonly Lazy<MediaSessionManager> _instance = new(() => new MediaSessionManager());

    private int _currentMediaSessionId;

    public MediaSessionManager()
    {
        MediaSessionInfos = new Dictionary<int, MediaInfo>();
    }

    public static MediaSessionManager Instance => _instance.Value;

    public Dictionary<int, MediaInfo> MediaSessionInfos { get; }

    public event PropertyChangedEventHandler PropertyChanged;

    public void HandleMediaSessionEvent(MediaSessionEvent mediaSessionEvent)
    {
        switch (mediaSessionEvent.EventType)
        {
            case EventType.NewSession:
                AddNewMediaSession(mediaSessionEvent);
                break;
            case EventType.CloseSession:
                RemoveMediaSession(mediaSessionEvent);
                break;
            case EventType.SongChanged:
                ChangeMediaSong(mediaSessionEvent);
                break;
            case EventType.PlaybackStatusChanged:
                ChangePlaybackStatus(mediaSessionEvent);
                break;
            case EventType.SessionFocusChanged:
                ChangeMediaSessionFocus(mediaSessionEvent);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void AddNewMediaSession(MediaSessionEvent mediaSession)
    {
        // Handle new session event
        var mediaInfo = new MediaInfo
        {
            PlayerId = mediaSession.MediaSessionId,
            PlayerName = mediaSession.MediaSessionName
        };

        _currentMediaSessionId = mediaInfo.PlayerId;

        MediaSessionInfos[mediaInfo.PlayerId] = mediaInfo;
        OnPropertyChanged(nameof(MediaSessionInfos));
    }

    private void RemoveMediaSession(MediaSessionEvent mediaSession)
    {
        // Handle close session event
        MediaSessionInfos.Remove(mediaSession.MediaSessionId);
        OnPropertyChanged(nameof(MediaSessionInfos));
    }

    private void ChangeMediaSong(MediaSessionEvent mediaSession)
    {
        // Handle song changed event
        if (!MediaSessionInfos.TryGetValue(mediaSession.MediaSessionId, out var mediaSessionInfo)) return;

        if (!string.IsNullOrEmpty(mediaSession.Artist))
            mediaSessionInfo.Artist = mediaSession.Artist;

        if (!string.IsNullOrEmpty(mediaSession.SongName))
            mediaSessionInfo.SongName = mediaSession.SongName;

        if (!string.IsNullOrEmpty(mediaSession.PlaybackStatus))
            mediaSessionInfo.PlaybackStatus = mediaSession.PlaybackStatus;

        OnPropertyChanged(nameof(MediaSessionInfos));
    }

    private void ChangePlaybackStatus(MediaSessionEvent mediaSession)
    {
        // Handle playback status changed event
        if (!MediaSessionInfos.TryGetValue(mediaSession.MediaSessionId, out var mediaSessionInfo)) return;

        if (!string.IsNullOrEmpty(mediaSession.Artist))
            mediaSessionInfo.Artist = mediaSession.Artist;

        if (!string.IsNullOrEmpty(mediaSession.SongName))
            mediaSessionInfo.SongName = mediaSession.SongName;

        if (!string.IsNullOrEmpty(mediaSession.PlaybackStatus))
            mediaSessionInfo.PlaybackStatus = mediaSession.PlaybackStatus;

        //IdLabel.Text = $"Playback updated: [{mediaSession.MediaSessionId}]";
        //MediaSessionLabel.Text = $"Playback updated: [{mediaSession.MediaSessionName}]";
        //ArtistLabel.Text = mediaSession.Artist;
        //SongNameLabel.Text = mediaSession.SongName;
        //PlaybackStatusLabel.Text = mediaSession.PlaybackStatus;

        OnPropertyChanged(nameof(MediaSessionInfos));
    }

    private void ChangeMediaSessionFocus(MediaSessionEvent mediaSession)
    {
        _currentMediaSessionId = mediaSession.MediaSessionId;
        // Remiantis situo, galima nustatyti kokia daina yra "pagrindine"
        // Handle session focus changed event (optional)
        //IdLabel.Text = $"Focus changed: [{mediaSession.MediaSessionId}]";
        //MediaSessionLabel.Text = $"Focus changed: [{mediaSession.MediaSessionName}]";
        //ArtistLabel.Text = mediaSession.Artist;
        //SongNameLabel.Text = mediaSession.SongName;
        //PlaybackStatusLabel.Text = mediaSession.PlaybackStatus;
        OnPropertyChanged(nameof(MediaSessionInfos));
    }

    public MediaInfo GetActiveMediaSession()
    {
        MediaSessionInfos.TryGetValue(_currentMediaSessionId, out var activeMediaSession);
        return activeMediaSession ?? new MediaInfo();
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}