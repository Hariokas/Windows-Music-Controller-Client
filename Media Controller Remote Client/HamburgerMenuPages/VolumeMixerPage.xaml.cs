using Media_Controller_Remote_Client.Models;

namespace Media_Controller_Remote_Client.HamburgerMenuPages;

public partial class VolumeMixerPage : ContentPage
{
    public VolumeMixerPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var viewModel = (VolumeMixerViewModel)BindingContext;
        _ = viewModel.LoadVolumes();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        var viewModel = (VolumeMixerViewModel)BindingContext;
        viewModel.UnsubscribeEvents();
    }

    void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
    {
        var slider = sender as Slider;
        var application = slider.BindingContext as ApplicationVolume;
        application.Volume = (float)e.NewValue;
        var viewModel = (VolumeMixerViewModel)BindingContext;
        viewModel.UpdateVolumeCommand.Execute(application);
    }

}