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

namespace Cloud_Backup_Core.Viewmodels
{
    internal class MainViewModel : BaseViewModel
    {
        #region RELAY COMMANDS

        public RelayCommand OpenBackupSettings_command => new RelayCommand(execute => OpenBackupSettings(), canExecute => true);
        public RelayCommand OpenLocalSettings_command => new RelayCommand(execute => OpenLocalSettings(), canExecute => true);
        public RelayCommand OpenFtpSettings_command => new RelayCommand(execute => OpenFtpSettings(), canExecute => true);
        //public RelayCommand EnterPressed_command => new RelayCommand(execute => EnterPressed(), canExecute => true);
        public RelayCommand ForceUpload_command => new RelayCommand(execute => ForceUpload(), canExecute => true);
        public RelayCommand StartSync_command => new RelayCommand(async execute => await StartSync(), canExecute => true);
        public RelayCommand PauseSync_command => new RelayCommand(execute => PauseSync(), canExecute => true);

        #endregion

        #region PROPERTIES DECLARATIONS

        private SettingsBackupViewModel bvm { get; set; }
        private SettingsLocalViewModel svm { get; set; }

        public FtpUploader FtpManager { get; }
        private List<CancellationTokenSource> UploadTokens { get; set; }

        private const int SYNC_TIMER = 2;
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


        private BACKUP_STATUS backupStatus;
        public BACKUP_STATUS BackupStatus
        {
            get { return backupStatus; }
            set
            {
                backupStatus = value;
                OnPropertyChanged(nameof(BackupStatus));
            }
        }

        private List<DispatcherTimer> BackupTimers;
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
        public enum BACKUP_STATUS
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
            BackupStatus = BACKUP_STATUS.IDLE;
            UploadTokens = new List<CancellationTokenSource>();

            bvm = new SettingsBackupViewModel();
            svm = new SettingsLocalViewModel();

            BackupTimers = new List<DispatcherTimer>();
            RootDirectory = @"C:\Users\paokf\Documents\root_upload";

            Task.Run(async () => await StartSync());
            AppVersion = GetAppVersion();
        }
        #endregion

        #region FUNCTIONS

        private string GetAppVersion()
        {
            string version = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings", "version.txt"));
            return $"Version: {version}";
        }

        private string SetBackupStatus(BACKUP_STATUS status) => status switch
        {
            BACKUP_STATUS.IDLE => "Idle",
            BACKUP_STATUS.ONLINE => "Online",
            _ => "Error"
        };

        private void PauseSync()
        {
            foreach (var timer in BackupTimers)
            {
                timer.Stop();
            }
            BackupTimers.Clear();
            ToBeUploaded.Clear();
            foreach (var cts in UploadTokens)
            {
                cts.Cancel();
            }
            UploadTokens.Clear();
            BackupStatus = BACKUP_STATUS.IDLE;
            Logger.Info("Backup paused");
        }

        private DispatcherTimer timer;
        private async Task StartSync()
        {
            var backupTimer = new DispatcherTimer();
            backupTimer.Interval = TimeSpan.FromMinutes(SYNC_TIMER);
            BackupTimers.Add(backupTimer);

            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMinutes(240);
            timer.Tick += async (o, s) => await SyncNow();
            timer.Start();
            BackupStatus = BACKUP_STATUS.ONLINE;

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
                BackupStatus = BACKUP_STATUS.IDLE;
            }
        }

        private async Task SyncNow()
        {
            ToBeUploaded = new List<string>();

            FtpSettings sfm = SettingsFileManager.LoadSettings<FtpSettings>(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings", MainWindow.SETTINGS_FTP_JSON));

            if (sfm?.UploadSettings == null || sfm.UploadSettings.Count == 0)
            {
                Logger.Error("No upload settings found.");
                return;
            }

            string user = sfm.RegisteredName;
            var globalCts = new CancellationTokenSource();
            UploadTokens.Add(globalCts);
            
            BackupStatus = BACKUP_STATUS.UPLOADING;

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
            BackupStatus = BACKUP_STATUS.IDLE;
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
