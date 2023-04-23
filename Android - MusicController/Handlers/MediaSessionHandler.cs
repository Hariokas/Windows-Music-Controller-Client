using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Android___MusicController.EventClasses;
using Android___MusicController.Models;

namespace Android___MusicController.Handlers;

public class MediaSessionHandler
{
    private static readonly Lazy<MediaSessionHandler> _instance = new Lazy<MediaSessionHandler>(() => new MediaSessionHandler());
    private int _currentMediaSessionId;

    public static MediaSessionHandler Instance { get { return _instance.Value; } }

    public MediaSessionHandler()
    {
        MediaSessionInfos = new Dictionary<int, MediaInfo>();
    }

    public Dictionary<int, MediaInfo> MediaSessionInfos { get; }

    public event PropertyChangedEventHandler PropertyChanged;
    public event EventHandler<PlaybackStatusChangedEventArgs> PlaybackStatusChanged;

    public void HandleMediaSessionEvent(MediaSessionEvent mediaSessionEvent)
    {
        try
        {
            switch (mediaSessionEvent.MediaSessionEventType)
            {
                case MediaSessionEventType.NewSession:
                    AddNewMediaSession(mediaSessionEvent);
                    break;
                case MediaSessionEventType.CloseSession:
                    RemoveMediaSession(mediaSessionEvent);
                    break;
                case MediaSessionEventType.SongChanged:
                    ChangeMediaSong(mediaSessionEvent);
                    break;
                case MediaSessionEventType.PlaybackStatusChanged:
                    ChangePlaybackStatus(mediaSessionEvent);
                    break;
                case MediaSessionEventType.SessionFocusChanged:
                    ChangeMediaSessionFocus(mediaSessionEvent);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex.Message);
        }
    }

    private void AddNewMediaSession(MediaSessionEvent mediaSession)
    {
        try
        {
            if (MediaSessionInfos.ContainsKey(mediaSession.MediaSessionId))
            {
                Trace.WriteLine($"{nameof(MediaSessionInfos)} already contains a media session with ID {mediaSession.MediaSessionId}");
                return;
            }

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
        catch (Exception ex)
        {
            Trace.WriteLine(ex.Message);
        }
    }

    private void RemoveMediaSession(MediaSessionEvent mediaSession)
    {
        try
        {
            // Handle close session event
            MediaSessionInfos.Remove(mediaSession.MediaSessionId);
            OnPropertyChanged(nameof(MediaSessionInfos));
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex.Message);
        }
    }

    private void ChangeMediaSong(MediaSessionEvent mediaSession)
    {
        try
        {
            // Handle song changed event
            if (!MediaSessionInfos.TryGetValue(mediaSession.MediaSessionId, out var mediaSessionInfo))
                return;

            //MediaSessionInfos.TryGetValue(mediaSession.MediaSessionId, out mediaSessionInfo);

            if (!string.IsNullOrEmpty(mediaSession.MediaSessionName))
                mediaSessionInfo.PlayerName = mediaSession.MediaSessionName;

            if (!string.IsNullOrEmpty(mediaSession.Artist))
                mediaSessionInfo.Artist = mediaSession.Artist;

            if (!string.IsNullOrEmpty(mediaSession.SongName))
                mediaSessionInfo.SongName = mediaSession.SongName;

            if (!string.IsNullOrEmpty(mediaSession.PlaybackStatus))
                mediaSessionInfo.PlaybackStatus = mediaSession.PlaybackStatus;

            mediaSessionInfo.CurrentPlaybackTime = mediaSession.CurrentPlaybackTime;
            mediaSessionInfo.SongLength = mediaSession.SongLength;

            var timeDifference = DateTime.UtcNow - mediaSession.Timestamp;
            mediaSessionInfo.CurrentPlaybackTime = mediaSession.CurrentPlaybackTime + timeDifference;
            mediaSessionInfo.SongLength = mediaSession.SongLength;

            OnPlaybackStatusChanged(mediaSession.PlaybackStatus);
            OnPropertyChanged(nameof(MediaSessionInfos));
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex.Message);
        }
    }

    private void ChangePlaybackStatus(MediaSessionEvent mediaSession)
    {
        try
        {
            // Handle playback status changed event
            if (!MediaSessionInfos.TryGetValue(mediaSession.MediaSessionId, out var mediaSessionInfo))
                return;

            //MediaSessionInfos.TryGetValue(mediaSession.MediaSessionId, out mediaSessionInfo);

            if (!string.IsNullOrEmpty(mediaSession.MediaSessionName))
                mediaSessionInfo.PlayerName = mediaSession.MediaSessionName;

            if (!string.IsNullOrEmpty(mediaSession.Artist))
                mediaSessionInfo.Artist = mediaSession.Artist;

            if (!string.IsNullOrEmpty(mediaSession.SongName))
                mediaSessionInfo.SongName = mediaSession.SongName;

            if (!string.IsNullOrEmpty(mediaSession.PlaybackStatus))
                mediaSessionInfo.PlaybackStatus = mediaSession.PlaybackStatus;

            var timeDifference = DateTime.UtcNow - mediaSession.Timestamp;
            mediaSessionInfo.CurrentPlaybackTime = mediaSession.CurrentPlaybackTime + timeDifference;
            mediaSessionInfo.SongLength = mediaSession.SongLength;

            OnPlaybackStatusChanged(mediaSession.PlaybackStatus);
            OnPropertyChanged(nameof(MediaSessionInfos));
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex.Message);
        }
    }

    protected virtual void OnPlaybackStatusChanged(string playbackStatus)
    {
        PlaybackStatusChanged?.Invoke(this, new PlaybackStatusChangedEventArgs(playbackStatus));
    }

    private void ChangeMediaSessionFocus(MediaSessionEvent mediaSession)
    {
        try
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
        catch (Exception ex)
        {
            Trace.WriteLine(ex.Message);
        }
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