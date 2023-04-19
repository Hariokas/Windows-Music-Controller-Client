using System.Diagnostics;
using Newtonsoft.Json;

namespace Android___MusicController;

public partial class MainPage : ContentPage
{
    private readonly WebSocketService _webSocketService;
    private readonly MainPageViewModel _viewModel;

    public MainPage()
    {
        InitializeComponent();
        _webSocketService = new WebSocketService("192.168.0.107", "8080");

        _viewModel = new MainPageViewModel(_webSocketService);
        this.BindingContext = _viewModel;
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
}