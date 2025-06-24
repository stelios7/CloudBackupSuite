using Cloud_Backup_Core.Helpers;
using System.Diagnostics;
using System.IO;
using System.Windows.Threading;
using System.Text;
using System.Windows.Controls;
using Cloud_Backup_Core.Views;
using Cloud_Backup_Core.Models;
using System.Windows;
using Cloud_Backup_Core.Helpers.Settings;
using Cloud_Backup_Core.Models.Settings;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using Updater.Core.Models;

namespace Cloud_Backup_Core.Viewmodels
{
    public class MainViewModel : BaseViewModel
    {
        #region RELAY COMMANDS

        public RelayCommand OpenBackupSettings_command => new RelayCommand(execute => OpenBackupSettings(), canExecute => true);
        public RelayCommand OpenLocalSettings_command => new RelayCommand(execute => OpenLocalSettings(), canExecute => true);
        public RelayCommand OpenFtpSettings_command => new RelayCommand(execute => OpenFtpSettings(), canExecute => true);
        //public RelayCommand EnterPressed_command => new RelayCommand(execute => EnterPressed(), canExecute => true);
        public RelayCommand ForceUpload_command => new RelayCommand(execute => ForceUpload(), canExecute => true);
        public RelayCommand StartSync_command => new RelayCommand(async execute => await StartSync(), canExecute => true);
        public RelayCommand PauseSync_command => new RelayCommand(execute => PauseSync(), canExecute => true);

        private void PauseSync()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region PROPERTIES DECLARATIONS

        private SettingsBackupViewModel bvm { get; set; }
        private SettingsLocalViewModel svm { get; set; }

        public FtpUploader FtpManager { get; }
        private List<CancellationTokenSource> UploadTokens { get; set; }

        private const int UPLOAD_SYNC_TIMER = 2;
        private string appVersion;

        [Required]
        public string AppVersion
        {
            get { return appVersion; }
            set { appVersion = value;
                OnPropertyChanged(nameof(AppVersion));
            }
        }

        private string fbu;

        public string FileBeingUploaded
        {
            get { return fbu; }
            set { fbu = value; 
                OnPropertyChanged(nameof(FileBeingUploaded));
            }
        }


        private string lastBackupTime;
        public string LastBackupTime
        {
            get { return lastBackupTime; }
            set
            {
                lastBackupTime = value;
                OnPropertyChanged(nameof(LastBackupTime));
            }
        }


        private STATE_STATUS backupStatus;
        public STATE_STATUS PROGRAM_STATUS
        {
            get { return backupStatus; }
            set
            {
                backupStatus = value;
                OnPropertyChanged(nameof(PROGRAM_STATUS));
            }
        }

        private List<DispatcherTimer> PROGRAM_TIMERS;
        public string RootDirectory { get; set; }

        private double upv;
        public double UploadProgressValue
        {
            get { return upv; }
            set
            {
                upv = value;
                OnPropertyChanged(nameof(UploadProgressValue));
            }
        }

        private string rootPassword;
        private List<string> ToBeUploaded;

        public string RootPassword
        {
            get { return rootPassword; }
            set
            {
                rootPassword = value;
                OnPropertyChanged(RootPassword);
            }
        }
        public enum STATE_STATUS
        {
            IDLE,
            ONLINE,
            UPLOADING
        }

        #endregion

        #region CONSTRUCTOR
        public MainViewModel()
        {
            // Debug.Print("Before instance");
            FtpManager = FtpUploader.Instance;
            PROGRAM_STATUS = STATE_STATUS.IDLE;
            UploadTokens = new List<CancellationTokenSource>();

            bvm = new SettingsBackupViewModel();
            svm = new SettingsLocalViewModel();

            PROGRAM_TIMERS = new List<DispatcherTimer>();
            RootDirectory = @"C:\Users\paokf\Documents\root_upload";

            SyncManager syncManager = new SyncManager(TimeSpan.FromMinutes(UPLOAD_SYNC_TIMER));
            syncManager.StatusChanged += status => PROGRAM_STATUS = status;
            syncManager.StartAsync().ConfigureAwait(false);

            AppVersion = GetAppVersion();
        }
        #endregion

        #region FUNCTIONS

        private string GetAppVersion()
        {
            // Assuming the version file is located in the settings/updater directory
            string versionFilePath = Path.Combine(AppContext.BaseDirectory, "settings", "updater", "core.json");

            if (!File.Exists(versionFilePath))
            {
                Logger.Error($"Version file not found: {versionFilePath}");
                return "Version file not found";
            }
            var core = JsonSerializer.Deserialize<AppUpdateConfig>(File.ReadAllText(versionFilePath));
            string version = core.CurrentVersion;
            return $"Version: {version}";
        }

        private string SetBackupStatus(STATE_STATUS status) => status switch
        {
            STATE_STATUS.IDLE => "Idle",
            STATE_STATUS.ONLINE => "Online",
            _ => "Error"
        };

        
        private DispatcherTimer timer;
        private async Task StartSync()
        {
            var backupTimer = new DispatcherTimer();
            backupTimer.Interval = TimeSpan.FromMinutes(UPLOAD_SYNC_TIMER);
            PROGRAM_TIMERS.Add(backupTimer);

            var uploadTimer = new DispatcherTimer();
            uploadTimer.Interval = TimeSpan.FromMinutes(UPLOAD_SYNC_TIMER);
            uploadTimer.Tick += async (o, s) => await SyncNow();
            uploadTimer.Start();

            foreach (var timer in PROGRAM_TIMERS)
            {
                timer.Start();
            }

            PROGRAM_STATUS = STATE_STATUS.ONLINE;

            Logger.Debug("Start syncing.");
            try
            {
                await Task.Run(() => SyncNow());
            }
            catch (Exception ex)
            {
                Logger.Error($"Sync failed: {ex.Message}");
            }
            finally
            {
                PROGRAM_STATUS = STATE_STATUS.IDLE;
            }
        }

        private async Task SyncNow()
        {
            ToBeUploaded = new List<string>();

            FtpSettings sfm = SettingsFileManager.LoadSettings<FtpSettings>(Path.Combine(AppContext.BaseDirectory, "settings", MainWindow.SETTINGS_FTP_JSON));

            if (sfm?.UploadSettings == null || sfm.UploadSettings.Count == 0)
            {
                Logger.Error("No upload settings found.");
                return;
            }

            if (sfm.RootFtpUploadDirectory == null || sfm.RootFtpUploadDirectory.Length == 0)
            {
                sfm.RootFtpUploadDirectory = "/CLOUDBACKUP/UPLOADS";
            }

            if (sfm.RemoteUpdateDirectory == null || sfm.RemoteUpdateDirectory.Length == 0)
            {
                sfm.RemoteUpdateDirectory = "/CLOUDBACKUP/UPDATE";
            }

            string user = sfm.RegisteredName;
            var globalCts = new CancellationTokenSource();
            UploadTokens.Add(globalCts);
            
            PROGRAM_STATUS = STATE_STATUS.UPLOADING;

            foreach (var item in sfm.UploadSettings.Where(s => s.IsUploadEnabled))
            {
                try
                {
                    var files = Directory.GetFiles(item.LocalPath);
                    foreach (var file in files)
                    {
                        FileBeingUploaded = Path.GetFileName(file);
                        
                        string ftpDestination = $"/{sfm.RootFtpUploadDirectory}/{item.Software}/{user}".Replace("//", "/");

                        try
                        {
                            await FtpManager.UploadFileFtp(file, ftpDestination, globalCts.Token);
                            Logger.Debug($"Upload successful: {file}");
                            FileMover.MoveFile(file); // Move only after successful upload
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"Upload failed for {file}: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error processing {item.Software}: {ex.Message}");
                }
            }
            PROGRAM_STATUS = STATE_STATUS.IDLE;
        }

        public void RootPasswordChanged(object sender, TextChangedEventArgs e)
        {
            Debug.WriteLine(e.ToString());
        }

        private void ForceUpload()
        {
            var settings = Properties.Settings.Default;
            StringBuilder s = new StringBuilder();
            s.AppendLine($"Sfuel: {settings.SfuelLocalFilepath}");
            s.AppendLine($"Lpg: {settings.LpgLocalFilePath}");
            s.AppendLine($"Softruck: {settings.SoftruckLocalFilePath}");
            s.AppendLine($"Upsales: {settings.UpsalesLocalFilePath}");

            LastBackupTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Task.Run(() => SyncNow());
            Logger.Debug(s.ToString());
        }

        // Opens the settings window for backup settings
        private void OpenBackupSettings()
        {
            OpenSettingsWindow<SettingsBackupView, SettingsBackupViewModel>();
        }

        // Opens the local settings window
        private void OpenLocalSettings()
        {
            OpenSettingsWindow<SettingsWindowLocalView, SettingsLocalViewModel>();
        }

        // Opens the FTP settings window
        private void OpenFtpSettings()
        {
            OpenSettingsWindow<SettingsWindowFtpView, SettingsFtpViewModel>();
        }

        #endregion
    }
}
