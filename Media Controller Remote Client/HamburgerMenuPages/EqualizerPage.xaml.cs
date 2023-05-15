using Media_Controller_Remote_Client.Handlers;

namespace Media_Controller_Remote_Client;

public partial class EqualizerPage : ContentPage
{
    private readonly WebSocketHandler _webSocketHandler = WebSocketHandler.Instance;

    public EqualizerPage()
    {
        InitializeComponent();
    }
}