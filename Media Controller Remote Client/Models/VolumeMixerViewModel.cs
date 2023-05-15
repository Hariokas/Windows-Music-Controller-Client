using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using Media_Controller_Remote_Client.EventClasses;
using Media_Controller_Remote_Client.Handlers;
using Microsoft.Maui.Controls;
using Newtonsoft.Json;

namespace Media_Controller_Remote_Client.Models
{
    public class VolumeMixerViewModel
    {
        public ObservableCollection<ApplicationVolume> ApplicationVolumes { get; set; }
        public ICommand UpdateVolumeCommand { get; set; }

        private readonly WebSocketHandler _webSocketHandler;

        public VolumeMixerViewModel()
        {
            try
            {
                ApplicationVolumes = new ObservableCollection<ApplicationVolume>();
                UpdateVolumeCommand = new Command<ApplicationVolume>(application =>
                {
                    Task.Run(() => UpdateVolume(application));
                });

                _webSocketHandler = WebSocketHandler.Instance;
                _webSocketHandler.VolumeMixerInfoReceived += VolumeMixerInfoReceived;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[VolumeMixerViewModel]: {ex}");
            }
        }

        public async Task LoadVolumes()
        {
            try
            {
                var volumeMixerEvent = new VolumeMixerEvent
                {
                    VolumeMixerEventType = VolumeMixerEventType.GetApplicationVolumes
                };

                var message = JsonConvert.SerializeObject(volumeMixerEvent);
                await _webSocketHandler.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[VolumeMixerViewModel]: {ex}");
            }
        }

        public async Task UpdateVolume(ApplicationVolume application)
        {
            try
            {
                var volumeMixerEvent = new VolumeMixerEvent
                {
                    VolumeMixerEventType = VolumeMixerEventType.SetApplicationVolume,
                    ApplicationVolume = application
                };

                var message = JsonConvert.SerializeObject(volumeMixerEvent);
                await _webSocketHandler.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[VolumeMixerViewModel]: {ex}");
            }
        }

        private void VolumeMixerInfoReceived(object sender, VolumeMixerEventArgs e)
        {
            try
            {
                var volumeMixerEvent = e.VolumeMixerEvent;
                if (volumeMixerEvent.VolumeMixerEventType == VolumeMixerEventType.GetApplicationVolumes)
                {
                    ApplicationVolumes.Clear();
                    foreach (var applicationVolume in volumeMixerEvent.ApplicationVolumes)
                    {
                        ApplicationVolumes.Add(applicationVolume);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[VolumeMixerViewModel]: {ex}");
            }
        }

        public void UnsubscribeEvents()
        {
            _webSocketHandler.VolumeMixerInfoReceived -= VolumeMixerInfoReceived;
        }

    }
}