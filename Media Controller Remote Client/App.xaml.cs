namespace Media_Controller_Remote_Client;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
        Preferences.Set("IpAddress", "192.168.0.108");
        Preferences.Set("Port", "8080");
    }
}
