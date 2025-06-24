using Cloud_Backup_Core.Helpers;
using Cloud_Backup_Core.Models.Settings;
using Cloud_Backup_Core.Models;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Threading;
using static Cloud_Backup_Core.Viewmodels.MainViewModel;

namespace Cloud_Backup_Core.Models
{
    public class SyncManager
    {
        #region EVENTS

        public event Action<STATE_STATUS> StatusChanged;

        private STATE_STATUS _status = STATE_STATUS.IDLE;
        public STATE_STATUS Status
        {
            get => _status;
            private set
            {
                if (_status != value)
                {
                    _status = value;
                    StatusChanged?.Invoke(_status); // Notify external subscribers
                }
            }
        }

        #endregion
        private readonly TimeSpan _interval;
        private readonly CancellationTokenSource _cts = new();
        private readonly List<CancellationTokenSource> _uploadTokens = new();
        private DispatcherTimer _syncTimer;

        public string FileBeingUploaded { get; private set; }
        public List<string> ToBeUploaded { get; private set; }

        FtpUploader FtpManager => FtpUploader.Instance;
        FileMover FileMover => FileMover.Instance;

        public SyncManager(TimeSpan interval)
        {
            _interval = interval;
        }


        /// <summary>
        /// 
        /// Starts the sync process. If already running, it will not start again.
        public async Task StartAsync()
        {
            if (_syncTimer != null)
                return;

            Status = STATE_STATUS.ONLINE;
            Logger.Debug("Start syncing.");

            // Initial sync
            await RunSyncAsync();

            // Recurring timer
            _syncTimer = new DispatcherTimer { Interval = _interval };
            _syncTimer.Tick += (_, _) => _ = RunSyncAsync(); // fire and forget safely
            _syncTimer.Start();
        }

        public void Stop()
        {
            _cts.Cancel();
            _syncTimer?.Stop();
            _syncTimer = null;
            Status = STATE_STATUS.IDLE;
            Logger.Debug("Sync stopped.");
        }

        private async Task RunSyncAsync()
        {
            try
            {
                Status = STATE_STATUS.ONLINE;
                await SyncNow(_cts.Token);
            }
            catch (OperationCanceledException)
            {
                Logger.Info("Sync canceled.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Sync failed: {ex.Message}");
            }
            finally
            {
                Status = STATE_STATUS.IDLE;
            }
        }

        private async Task SyncNow(CancellationToken token)
        {
            ToBeUploaded = new List<string>();

            var settings = LoadValidFtpSettings();
            if (settings == null)
            {
                Logger.Error("Invalid FTP settings. Sync aborted.");
                return;
            }

            var globaCts = CancellationTokenSource.CreateLinkedTokenSource(token);
            _uploadTokens.Add(globaCts);

            string user = settings.RegisteredName;

            foreach (var uploadSetting in settings.UploadSettings.Where(s => s.IsUploadEnabled))
            {
                try
                {
                    var localFiles = Directory.GetFiles(uploadSetting.LocalPath);
                    string ftpDestination = GetUploadDestination(settings.RootFtpUploadDirectory, uploadSetting.Software, user);

                    foreach (var file in localFiles)
                    {
                        token.ThrowIfCancellationRequested();
                        FileBeingUploaded = Path.GetFileName(file);

                        await UploadFileAsync(file, ftpDestination, token);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error processing {uploadSetting.Software}: {ex.Message}");
                }
            }
            Logger.Info("Sync completed successfully.");
        }

        private FtpSettings LoadValidFtpSettings()
        {
            var settingsPath = MainWindow.SETTINGS_FTP_JSON;
            var sfm  = SettingsFileManager.LoadSettings<FtpSettings>(settingsPath);

            if (sfm == null || !sfm.UploadSettings.Any() || string.IsNullOrWhiteSpace(sfm.RegisteredName) || string.IsNullOrWhiteSpace(sfm.RootFtpUploadDirectory))
            {
                Logger.Error("FTP settings are invalid or incomplete.");
                return null;
            }

            return sfm;
        }

        private async Task UploadFileAsync(string file, string ftpDestination, CancellationToken token)
        {
            try
            {
                await FtpManager.UploadFileFtp(file, ftpDestination, token);
                Logger.Debug($"Upload successful: {file}");
                FileMover.MoveFile(file);
            }
            catch (Exception ex)
            {
                Logger.Error($"Upload failed for {file}: {ex.Message}");
            }
        }

        private string GetUploadDestination(string rootFtpUploadDirectory, string software, string user) => $"{rootFtpUploadDirectory}/{software}/{user}".Replace("//", "/");
    }
}
