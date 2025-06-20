using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Shapes = System.Windows.Shapes;
using System.Windows.Threading;
using Cloud_Backup_Core.Helpers;
using Cloud_Backup_Core.Viewmodels;
using System.Linq.Expressions;
using System.ComponentModel;

namespace Cloud_Backup_Core
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CancellationTokenSource _cts;
        private NotifyIcon _notifyIcon;

        private const int UPDATE_TIMER = 300;
        private MainViewModel _mainViewModel;

        public static readonly string SETTINGS_BACKUP_JSON = Path.Combine(AppContext.BaseDirectory, "settings", "backup_settings.json");
        public static readonly string SETTINGS_UPDATER_JSON = Path.Combine(AppContext.BaseDirectory, "settings", "updater_config.json");
        public static readonly string SETTINGS_LOCAL_JSON = Path.Combine(AppContext.BaseDirectory, "settings", "local_settings.json");
        public static readonly string SETTINGS_FTP_JSON = Path.Combine(AppContext.BaseDirectory, "settings", "ftp_settings.json");
        public static readonly string SETTINGS_VERSION_TXT = Path.Combine(AppContext.BaseDirectory, "settings", "version.txt");

        private DispatcherTimer _updateCheckTimer;
        private readonly string _updaterPath = Path.Combine(AppContext.BaseDirectory, "updater.exe");
        private readonly TimeSpan _updateCheckInterval = TimeSpan.FromMinutes(1);

        public MainWindow()
        {
            CreateNecessaryData();
            InitializeComponent();

            _mainViewModel = new MainViewModel();

            SetupTrayIcon();
            CreateTimers();

            this.Loaded += MainWindow_Loaded;
            this.DataContext = _mainViewModel;
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
        }

        private void CreateNecessaryData()
        {
            var appDomain = Path.Combine(AppContext.BaseDirectory);
            var appDomainSettings = Path.Combine(appDomain, "settings");

            // Δημιουργία φακέλου για την ενημέρωση του προγράμματος
            DirectoryInfo[] directoryInfos = new DirectoryInfo[]
            {
                new DirectoryInfo(Path.Combine(appDomain, "update", "temp")),
                new DirectoryInfo(Path.Combine(appDomain, "settings"))
            };
            foreach (var di in directoryInfos)
            {
                if (!di.Exists)
                {
                    di.Create();
                }
                di.Attributes |= FileAttributes.Directory | FileAttributes.Hidden;
            }

            // Δημιουργία αρχείων που θα χρησιμοποιηθούν για την ενημέρωση
            FileInfo[] fileInfos = new FileInfo[]
            {
                new FileInfo(SETTINGS_BACKUP_JSON),
                new FileInfo(SETTINGS_LOCAL_JSON),
                new FileInfo(SETTINGS_UPDATER_JSON),
                new FileInfo(SETTINGS_FTP_JSON),
                new FileInfo(SETTINGS_VERSION_TXT)
            };

            // Ελέγχω αν τα αρχεία υπάρχουν και αν όχι, τα δημιουργώ
            foreach (var file in fileInfos)
            {
                if (!file.Exists)
                {
                    file.Create().Close();
                }
            }
        }

        /// <summary>
        /// Initializes the system tray icon and sets up event handlers for property changes.
        /// </summary>
        private void SetupTrayIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = new Icon(System.AppContext.BaseDirectory + @"Resources\Content\cloud_backup.ico"),
                Visible = false,
                Text = $"Cloud Backup Service - Ενεργό" // Tooltip text
            };
            _mainViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(MainViewModel.BackupStatus))
                {
                    _notifyIcon.Text = $"Cloud Backup - {_mainViewModel.BackupStatus}";
                }
            };
            _notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
        }

        private void CreateTimers()
        {
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        // Run updater
                        RunUpdater();

                        while (!token.IsCancellationRequested)
                        {
                            await Task.Delay(TimeSpan.FromMinutes(1), token);

                            if (token.IsCancellationRequested)
                                break;

                            RunUpdater();
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        // Exit the loop if task is cancelled
                        Logger.Info("Updater task was cancelled.");
                        break;
                    }
                    catch (Exception ex)
                    {
                        // Log error or handle exception
                        Logger.Error($"Error: {ex.Message}");
                    }
                }
            }, token);
        }

        private readonly string UPDATER_EXE = Path.Combine(AppContext.BaseDirectory, "Updater.Client.exe");
        private readonly string UPDATER_EXE_DEBUG = "C:\\Users\\User\\Documents\\stelios\\Code\\Cloud Backup Core\\Updater.Client\\bin\\Debug\\net9.0-windows\\updater.client.exe";

        private void RunUpdater()
        {
            // Path to the updater executable
            string updaterPath = UPDATER_EXE;
#if DEBUG 
            updaterPath = UPDATER_EXE_DEBUG;
#endif

            // Check if updater exists
            if (!System.IO.File.Exists(updaterPath))
            {
                Logger.Warning("Updater not found.");
                return;
            }

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = updaterPath,
                    WorkingDirectory = Path.GetDirectoryName(updaterPath),
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                Process.Start(psi);
                Logger.Info("Updater launched");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to start updater: {ex.Message}");
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _cts.Cancel();
            base.OnClosing(e);
        }

        // Μπορώ να χειριστώ events Loaded, Closing, Closed
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            this.Hide();
            _notifyIcon.Visible = true;
        }
        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
                _notifyIcon.Visible = true;
            }
            else if (WindowState == WindowState.Normal)
            {
                _notifyIcon.Visible = false;
            }

            base.OnStateChanged(e);
        }
        protected override void OnClosed(EventArgs e)
        {
            _notifyIcon.Dispose();
            _notifyIcon = null;
            _cts.Cancel();
            base.OnClosed(e);
        }

    }
}