using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Android___MusicController.Handlers;
using Android___MusicController.Models;

namespace Android___MusicController;

public partial class SessionsPage : ContentPage, INotifyPropertyChanged
{
    private readonly MediaSessionHandler _mediaSessionManager;

    public ObservableCollection<MediaInfo> MediaSessions { get; set; }

    
    public event PropertyChangedEventHandler PropertyChanged;

    public SessionsPage()
    {
        InitializeComponent();
        //_mediaSessionManager = MediaSessionHandler.Instance;
        _mediaSessionManager = MediaSessionHandler.Instance;
        _mediaSessionManager.PropertyChanged += MediaSessionManager_PropertyChanged;
        MediaSessions = new ObservableCollection<MediaInfo>();
        UpdateMediaSessions();
        SessionsListView.ItemsSource = MediaSessions;
    }

    private void UpdateMediaSessions()
    {
        MediaSessions.Clear();
        foreach (var mediaInfo in _mediaSessionManager.MediaSessionInfos.Values) MediaSessions.Add(mediaInfo);
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void MediaSessionManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MediaSessionHandler.MediaSessionInfos))
        {
            UpdateMediaSessions();
        }
    }

}