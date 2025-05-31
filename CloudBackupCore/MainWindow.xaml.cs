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
using System.Windows.Shapes;
using System.Windows.Threading;
using Cloud_Backup_Core.Helpers;
using Cloud_Backup_Core.Viewmodels;

namespace Cloud_Backup_Core
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int UPDATE_TIMER = 300;
        private NotifyIcon _notifyIcon;
        private CancellationTokenSource _cts;
        MainViewModel vm;

        public MainWindow()
        {
            CreateNecessaryData();
            //InitializeComponent();
            SetupTrayIcon();
            CreateTimers();


            this.Loaded += MainWindow_Loaded;
            this.DataContext = vm;
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
        }
        private void CreateNecessaryData()
        {
            vm = new MainViewModel();

            // Φτιάχνω φάκελο στο APPDATA αν δεν υπάρχει ήδη
            string localAppData = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Cloud_Backup_Core");
            string appDataLocalFolder = System.IO.Path.Combine(localAppData, "update");
            if (!Directory.Exists(appDataLocalFolder))
            {
                Directory.CreateDirectory(appDataLocalFolder);
                Debug.Print("%LOCALAPPDATA% OK!");
            }

            // Φτιάχνω φάκελο temp στην τοποθεσία εγκατάστασης
            Config.Instance.Load(System.IO.Path.Combine(localAppData, "updater_config.json"));

            string tempFolder = System.IO.Path.Combine(Config.Instance.LocalAppPath, "temp");
            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
                Debug.Print("Temp folder OK");
            }

            // Φτιάχνω κρυφό φάκελο ρυθμίσεων στην τοποθεσία εγκατάστασης
            string folderPath = System.IO.Path.Combine(Config.Instance.LocalAppPath, "settings");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                Logger.Debug("Ο φάκελος ρυθμίσεων δημιουργήθηκε.");
            }

            // Ορισμός ιδιοτήτων του φακέλου σε κρυφές
            File.SetAttributes(folderPath, FileAttributes.Hidden);
            Logger.Debug("Ο φάκελος ρυθμίσεων έχει γίνει κρυφός.");
        }
        private void SetupTrayIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = new Icon(System.AppDomain.CurrentDomain.BaseDirectory + @"Resources\Content\cloud_backup.ico"),
                Visible = false,
                Text = $"Cloud Backup Service - Ενεργό" // Tooltip text
            };
            vm.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(MainViewModel.BackupStatus))
                {
                    _notifyIcon.Text = $"Cloud Backup - {vm.BackupStatus}"; 
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
                await Task.Delay(2000);
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        // Run updater
                        RunUpdater();

                        // Wait for 20 seconds
                        await Task.Delay(TimeSpan.FromSeconds(UPDATE_TIMER), token);
                    }
                    catch (TaskCanceledException)
                    {
                        // Exit the loop if task is cancelled
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
        private void RunUpdater()
        {
            Debug.Print("Updater started");
            // Path to the updater executable
            string updaterPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "CloudBackupCore", "CloudUpdater.exe");
            //string updaterPath = "notepad.exe";

            // Check if updater exists
            if (!System.IO.File.Exists(updaterPath))
            {
                Debug.Print("Updater not found.");
                return;
            }

            // Start the updater as a separate process
            var processInfo = new ProcessStartInfo
            {
                FileName = updaterPath,
                UseShellExecute = true,  // Required to run as administrator
                Verb = "runas"           // Run as admin
            };

            try
            {
                Process.Start(processInfo);
            }
            catch (Exception ex)
            {
                Debug.Print($"Failed to start updater: {ex.Message}");
            }
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