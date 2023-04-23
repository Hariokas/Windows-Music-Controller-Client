namespace Android___MusicController.Models;

public class MediaInfo
{
    public int PlayerId { get; set; }

    public string PlayerName { get; set; }

    public string Artist { get; set; }

    public string SongName { get; set; }

    public string PlaybackStatus { get; set; }

    public ImageSource AlbumArtImage { get; set; }
    public TimeSpan SongLength { get; set; }
    public TimeSpan CurrentPlaybackTime { get; set; }
}