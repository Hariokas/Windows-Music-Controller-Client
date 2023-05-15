using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using Media_Controller_Remote_Client.EventClasses;
using Newtonsoft.Json;

namespace Media_Controller_Remote_Client.Handlers;

public class WebSocketHandler
{
    private static readonly Lazy<WebSocketHandler> _lazyInstance = new(() => new WebSocketHandler());

    public string IpAddress => Preferences.Get("IpAddress", string.Empty);
    public string Port => Preferences.Get("Port", string.Empty);



    private ClientWebSocket _clientWebSocket;
    public EventHandler<MasterVolumeEventArgs> MasterVolumeInfoReceived;
    public EventHandler<MediaSessionEventArgs> MediaSessionInfoReceived;
    public EventHandler<byte[]> OnBinaryMessageReceived;
    public EventHandler<VolumeMixerEventArgs> VolumeMixerInfoReceived;

    public WebSocketHandler()
    {
        ConnectWebSocket();
    }

    public static WebSocketHandler Instance => _lazyInstance.Value;

    private async void ConnectWebSocket()
    {
        while (true)
        {
            try
            {
                _clientWebSocket?.Dispose();
                _clientWebSocket = new ClientWebSocket();

                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3)))
                {
                    Debug.WriteLine("Connecting to the host");
                    await _clientWebSocket.ConnectAsync(new Uri($"ws://{IpAddress}:{Port}"), cts.Token);
                }

                Debug.WriteLine($"Connected to host at ws://{IpAddress}:{Port}");
                await ReceiveMessagesAsync();
                Debug.WriteLine("Connection lost");
            }
            catch (WebSocketException ex)
            {
                Debug.WriteLine($"WebSocket Error: {ex.Message}");
            }
            catch (OperationCanceledException ex)
            {
                Debug.WriteLine($"Connection Timeout: {ex.Message}");
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

            // Reconnect after a delay
            await Task.Delay(TimeSpan.FromSeconds(3));
            Debug.WriteLine("Reconnecting to the host...");
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
                    var combinedImageData = StaticHelpers.CombineImageDataChunks(imageDataChunks);
                    ProcessBinaryMessage(combinedImageData);
                    imageDataChunks.Clear();
                }
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
        Trace.WriteLine($"Received message: {message}");
        try
        {
            var baseEvent = JsonConvert.DeserializeObject<BaseEvent>(message);

            switch (baseEvent.EventType)
            {
                case BaseEventType.MasterVolumeEvent:
                    var masterVolumeEvent = JsonConvert.DeserializeObject<MasterVolumeEvent>(message)
                                            ?? throw new ArgumentNullException(
                                                $"Failed to deserialize {nameof(MasterVolumeEvent)}");
                    MasterVolumeInfoReceived?.Invoke(this, new MasterVolumeEventArgs(masterVolumeEvent));
                    break;

                case BaseEventType.MediaSessionEvent:
                    var mediaSessionEvent = JsonConvert.DeserializeObject<MediaSessionEvent>(message)
                                            ?? throw new ArgumentNullException(
                                                $"Failed to deserialize {nameof(MasterVolumeEvent)}");
                    MediaSessionInfoReceived?.Invoke(this, new MediaSessionEventArgs(mediaSessionEvent));
                    break;

                case BaseEventType.VolumeMixerEvent:
                    var volumeMixerEvent = JsonConvert.DeserializeObject<VolumeMixerEvent>(message)
                                           ?? throw new ArgumentNullException(
                                               $"Failed to deserialize {nameof(MasterVolumeEvent)}");
                    VolumeMixerInfoReceived?.Invoke(this, new VolumeMixerEventArgs(volumeMixerEvent));
                    break;

                default:
                    Trace.WriteLine($"Unknown event type: {baseEvent.EventType}");
                    break;
            }
        }
        catch (Exception e)
        {
            Trace.Write($"Error: {e.Message}");
        }
    }

    private void ProcessBinaryMessage(byte[] message)
    {
        OnBinaryMessageReceived?.Invoke(this, message);
    }
}