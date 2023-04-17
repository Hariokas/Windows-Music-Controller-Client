using System.Diagnostics;
using System.Globalization;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Xml;
using Newtonsoft.Json;

namespace Android___MusicController;

public partial class MainPage : ContentPage
{
    public delegate void MediaSessionEventHandler(MediaSessionEvent mediaSessionEvent);

    //private string message = "Waiting for message...";
    private static readonly Dictionary<int, MediaInfo> MediaSessionInfos = new();

    private ClientWebSocket _clientWebSocket;

    public MainPage()
    {
        InitializeComponent();

        OnMediaSessionEvent += HandleMediaSessionEvent;

        ConnectWebSocket();
    }

    public event MediaSessionEventHandler OnMediaSessionEvent;

    private async void ConnectWebSocket()
    {
        while (true)
        {
            try
            {
                _clientWebSocket?.Dispose();
                _clientWebSocket = new ClientWebSocket();
                await _clientWebSocket.ConnectAsync(new Uri("ws://192.168.0.107:8080"), CancellationToken.None);
                await ReceiveMessagesAsync();
            }
            catch (WebSocketException ex)
            {
                Debug.WriteLine($"WebSocket Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                if (_clientWebSocket != null && _clientWebSocket.State != WebSocketState.Closed)
                    try
                    {
                        await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing websocket",
                            CancellationToken.None);
                    }
                    catch (WebSocketException ex)
                    {
                        Debug.WriteLine($"WebSocketException during close: {ex.Message}");
                    }
            }

            //reconnect after a delay
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }

    private async Task ReceiveMessagesAsync()
    {
        var buffer = new byte[1024];
        var imageDataChunks = new List<byte[]>();

        while (_clientWebSocket.State == WebSocketState.Open)
        {
            var result = await _clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Text)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                ProcessTextMessage(message);
            }
            else if (result.MessageType == WebSocketMessageType.Binary)
            {
                var imageDataChunk = new byte[result.Count];
                Array.Copy(buffer, imageDataChunk, result.Count);
                imageDataChunks.Add(imageDataChunk);

                if (result.EndOfMessage)
                {
                    var combinedImageData = CombineImageDataChunks(imageDataChunks);
                    LoadImageFromByteArray(combinedImageData);
                    imageDataChunks.Clear();
                }
                //var imageDate = new byte[result.Count];
                //Array.Copy(buffer, imageDate, result.Count);
                //LoadImageFromByteArray(imageDate);
            }
            else /*if (result.MessageType == WebSocketMessageType.Close)*/
            {
                await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing websocket",
                    CancellationToken.None);
            }
        }
    }

    public async Task SendMessageAsync(string message)
    {
        Debug.WriteLine($"Sending message: {message}");
        try
        {
            var data = Encoding.UTF8.GetBytes(message);
            var segment = new ArraySegment<byte>(data);

            await _clientWebSocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    private void ProcessTextMessage(string message)
    {
        Debug.WriteLine($"Received message: {message}");
        try
        {
            var mediaSessionEvent = JsonConvert.DeserializeObject<MediaSessionEvent>(message);
            OnMediaSessionEvent?.Invoke(mediaSessionEvent);
        }
        catch
        {
            UpdateLabel(message);
        }
    }

    private void HandleMediaSessionEvent(MediaSessionEvent mediaSessionEvent)
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
                UpdateUserInterface(mediaSessionEvent);
                break;
            case EventType.PlaybackStatusChanged:
                ChangePlaybackStatus(mediaSessionEvent);
                UpdateUserInterface(mediaSessionEvent);
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

        MediaSessionInfos[mediaInfo.PlayerId] = mediaInfo;

        //IdLabel.Text = $"New session: [{mediaSession.MediaSessionId}]";
        //MediaSessionLabel.Text = $"New session: [{mediaSession.MediaSessionName}]";
    }

    private void RemoveMediaSession(MediaSessionEvent mediaSession)
    {
        // Handle close session event
        MediaSessionInfos.Remove(mediaSession.MediaSessionId);
        //IdLabel.Text = $"Session closed: [{mediaSession.MediaSessionId}]";
        //MediaSessionLabel.Text = $"Session closed: [{mediaSession.MediaSessionName}]";
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

        //IdLabel.Text = $"Song changed: [{mediaSession.MediaSessionId}]";
        //MediaSessionLabel.Text = $"Song changed: [{mediaSession.MediaSessionName}]";
        //ArtistLabel.Text = mediaSession.Artist;
        //SongNameLabel.Text = mediaSession.SongName;
        //PlaybackStatusLabel.Text = mediaSession.PlaybackStatus;
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
    }

    private void ChangeMediaSessionFocus(MediaSessionEvent mediaSession)
    {
        // Remiantis situo, galima nustatyti kokia daina yra "pagrindine"
        // Handle session focus changed event (optional)
        //IdLabel.Text = $"Focus changed: [{mediaSession.MediaSessionId}]";
        //MediaSessionLabel.Text = $"Focus changed: [{mediaSession.MediaSessionName}]";
        //ArtistLabel.Text = mediaSession.Artist;
        //SongNameLabel.Text = mediaSession.SongName;
        //PlaybackStatusLabel.Text = mediaSession.PlaybackStatus;
    }

    private void UpdateUserInterface(MediaSessionEvent mediaSession)
    {
        if (!MediaSessionInfos.TryGetValue(mediaSession.MediaSessionId, out var mediaSessionInfo)) return;

        IdLabel.Text = mediaSessionInfo.PlayerId.ToString();
        MediaSessionLabel.Text = mediaSessionInfo.PlayerName;
        ArtistLabel.Text = mediaSessionInfo.Artist;
        SongNameLabel.Text = mediaSessionInfo.SongName;
        PlaybackStatusLabel.Text = mediaSessionInfo.PlaybackStatus;
    }

    private byte[] CombineImageDataChunks(List<byte[]> imageDataChunks)
    {
        var combinedImageData = new byte[imageDataChunks.Sum(chunk => chunk.Length)];
        var offset = 0;

        foreach (var chunk in imageDataChunks)
        {
            Buffer.BlockCopy(chunk, 0, combinedImageData, offset, chunk.Length);
            offset += chunk.Length;
        }

        return combinedImageData;
    }

    private void LoadImageFromByteArray(byte[] imageDate)
    {
        Debug.WriteLine($"Received image of size: {imageDate.Length}");
        Dispatcher.Dispatch(() =>
        {
            var imageSource = ImageSource.FromStream(() => new MemoryStream(imageDate));
            AlbumArt.Source = imageSource;
        });

        //_ = SaveImageToFileAsync(imageDate, $"{DateTime.Now:yyyyMMdd-HHmmss}_received_image.png");
    }

    private async Task SaveImageToFileAsync(byte[] imageDate, string fileName)
    {
        ///data/user/0/com.kakisanovichirko.android___musiccontroller/files/20230415-220828_received_image.png
        var appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var filePath = Path.Combine(appDataDirectory, fileName);

        await File.WriteAllBytesAsync(filePath, imageDate);
        Debug.WriteLine($"Image saved to: {filePath}");
    }

    private void UpdateLabel(string message)
    {
        //WebSocketLbl.Text = message;
    }

    private async void PlayPauseButton_OnClicked(object sender, EventArgs e)
    {
        var mediaSessionEvent = new MediaSessionEvent { EventType = EventType.Play };
        var message = JsonConvert.SerializeObject(mediaSessionEvent);
        await SendMessageAsync(message);
    }

    private async void PreviousButton_OnClicked(object sender, EventArgs e)
    {
        var mediaSessionEvent = new MediaSessionEvent { EventType = EventType.Previous };
        var message = JsonConvert.SerializeObject(mediaSessionEvent);
        await SendMessageAsync(message);
    }

    private async void NextButton_OnClicked(object sender, EventArgs e)
    {
        var mediaSessionEvent = new MediaSessionEvent { EventType = EventType.Next };
        var message = JsonConvert.SerializeObject(mediaSessionEvent);
        await SendMessageAsync(message);
    }

    //private void OnCounterClicked(object sender, EventArgs e)
    //{
    //    count++;

    //    if (count == 1)
    //        CounterBtn.Text = $"Clicked {count} time";
    //    else
    //        CounterBtn.Text = $"Clicked {count} times";

    //    SemanticScreenReader.Announce(CounterBtn.Text);
    //}

    public class MediaInfo
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
        public string Artist { get; set; }
        public string SongName { get; set; }
        public string PlaybackStatus { get; set; }
    }

    public class MusicInfo
    {
        public int Id { get; set; }
        public string Artist { get; set; }
        public string MediaSessionName { get; set; }
        public string PlaybackStatus { get; set; }
        public string SongName { get; set; }
    }

    [ContentProperty("Source")]
    public class ImageResourceExtension : IMarkupExtension<ImageSource>
    {
        public string Source { set; get; }

        public ImageSource ProvideValue(IServiceProvider serviceProvider)
        {
            if (String.IsNullOrEmpty(Source))
            {
                IXmlLineInfoProvider lineInfoProvider = serviceProvider.GetService(typeof(IXmlLineInfoProvider)) as IXmlLineInfoProvider;
                IXmlLineInfo lineInfo = (lineInfoProvider != null) ? lineInfoProvider.XmlLineInfo : new XmlLineInfo();
                throw new XamlParseException("ImageResourceExtension requires Source property to be set", lineInfo);
            }

            string assemblyName = GetType().GetTypeInfo().Assembly.GetName().Name;
            return ImageSource.FromResource(assemblyName + "." + Source, typeof(ImageResourceExtension).GetTypeInfo().Assembly);
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        {
            return (this as IMarkupExtension<ImageSource>).ProvideValue(serviceProvider);
        }
    }
}