using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using System.Windows.Input;

namespace Media_Controller_Remote_Client.HamburgerMenuPages;

public partial class SettingsPage : ContentPage
{
    public ICommand SaveCommand { get; set; }

    private string _ipAddress = Preferences.Get("IpAddress", string.Empty);
    private string _port = Preferences.Get("Port", string.Empty);

    public SettingsPage()
    {
        SaveCommand = new Command(() =>
        {
            Preferences.Set("IpAddress", IpAddress);
            Preferences.Set("Port", Port);
            DisplayAlert("Success", "Settings Saved Successfully", "OK");
        });

        InitializeComponent();
        this.BindingContext = this;

        IpAddress = Preferences.Get("IpAddress", string.Empty);
        Port = Preferences.Get("Port", string.Empty);
    }

    public string IpAddress
    {
        get => _ipAddress;
        set
        {
            if (_ipAddress != value)
            {
                _ipAddress = value;
                OnPropertyChanged();
            }
        }
    }

    public string Port
    {
        get => _port;
        set
        {
            if (_port != value)
            {
                _port = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}