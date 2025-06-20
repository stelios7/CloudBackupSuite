using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.OleDb;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Cloud_Backup_Core.Helpers;
using Cloud_Backup_Core.Models;
using Cloud_Backup_Core.Models.Settings;
using Cloud_Backup_Core.Viewmodels;
using FluentFTP;
using FluentFTP.Exceptions;

namespace Cloud_Backup_Core.Viewmodels
{
    internal class SettingsFtpViewModel : BaseViewModel
    {
        #region DECLARATIONS

        private const int MAXIMUM_UPLOAD_SETTINGS = 6;
        private string ftpUsername;

        public string FtpUsername
        {
            get { return ftpUsername; }
            set
            {
                ftpUsername = value;
                OnPropertyChanged(nameof(FtpUsername));
            }
        }

        private string ftpServerAddress;
        public string FtpServerAddress
        {
            get { return ftpServerAddress; }
            set
            {
                ftpServerAddress = value;
                OnPropertyChanged(nameof(FtpServerAddress));
            }
        }

        private string ftpPort;
        public string FtpPort
        {
            get { return ftpPort; }
            set
            {
                ftpPort = value;
                OnPropertyChanged(nameof(FtpPort));
            }
        }

        private string _ftpPassword;
        public string FtpPassword
        {
            get { return _ftpPassword; }
            set
            {
                _ftpPassword = value;
                OnPropertyChanged(nameof(FtpPassword));
            }
        }

        private string _registeredName;

        public string RegisteredName
        {
            get { return _registeredName; }
            set
            {
                _registeredName = value;
                OnPropertyChanged(nameof(RegisteredName));
            }
        }

        private readonly string FtpSettingsFilePath = Path.Combine(AppContext.BaseDirectory, "Settings", "ftp_settings.json");
        public static FtpSettings FtpSettings { get; set; }

        public RelayCommand AddUploadSettingCommand => new RelayCommand(execute => AddSetting(), canExecute => CanAddSetting());
        public RelayCommand RemoveUploadSettingCommand => new RelayCommand(execute => RemoveSetting(), canExecute => CanRemoveSetting());
        public RelayCommand TestFtpConnectionCommand => new RelayCommand(async execute => await TestFtpConnection(), canExecute => CanTestFtpConnection());

        private bool isTryingFtpConnection = false;

        private bool CanTestFtpConnection()
        {
            if (FtpServerAddress == null || FtpUsername == null || FtpPassword == null || FtpPort == null) return false;
            return FtpServerAddress.Length > 0 && FtpUsername.Length > 0 && FtpPassword.Length > 0 && FtpPort.Length > 0 && !isTryingFtpConnection;
        }

        private async Task TestFtpConnection()
        {
            try
            {
                isTryingFtpConnection = true;
                using var client = new AsyncFtpClient()
                {
                    Credentials = new NetworkCredential(FtpUsername, FtpPassword),
                    Port = Int32.Parse(FtpPort),
                    Host = FtpServerAddress
                };

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));

                await client.Connect(cts.Token);

                if (client.IsConnected)
                {
                    System.Windows.MessageBox.Show("FTP Connection successful!");
                }
                else
                {
                    System.Windows.MessageBox.Show("Ftp connection failed.");
                }
                isTryingFtpConnection = false;
            }
            catch (OperationCanceledException)
            {
                // connection timed out (after 3 seconds)
                System.Windows.MessageBox.Show("Connection timed out.");
                isTryingFtpConnection = false;
            }
            catch (FtpAuthenticationException ex)
            {
                // incorrect credentials
                System.Windows.MessageBox.Show("Authentication failed: " + ex.Message);
                isTryingFtpConnection = false;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Connection error: {ex.Message}");
                isTryingFtpConnection = false;
            }
        }

        private RelayCommand saveCommand;
        public RelayCommand SaveCommand
        {
            get { return saveCommand ?? (saveCommand = new RelayCommand(execute => { SetSettings(); SaveSettings(); }, canExecute => true)); }
        }

        public ObservableCollection<SettingsControlViewModel> UploadSettings { get; set; }

        #endregion

        #region CONSTRUCTOR

        public SettingsFtpViewModel()
        {
            LoadSettings();
        }

        #endregion

        #region FUNCTIONS

        private void SaveSettings()
        {
            SettingsFileManager.SaveSettings<FtpSettings>(FtpSettingsFilePath, FtpSettings);
            SaveAndClose();
        }

        private void SetSettings()
        {
            // Convert ObservableCollection<SettingsControlViewModel> to List<UploadSetting>
            FtpSettings.UploadSettings = this.UploadSettings.Select(setting => new UploadSetting(setting.UploadEnabled, setting.SoftwareName, setting.LocalPath)).ToList();
            FtpSettings.UseDefaultFtpCredentials = true; // Assuming we want to use default credentials
            FtpSettings.RegisteredName = this.RegisteredName;
            FtpSettings.ServerAddress = this.FtpServerAddress;
            FtpSettings.Username = this.FtpUsername;
            FtpSettings.Password = this.FtpPassword;
            FtpSettings.Port = int.TryParse(this.FtpPort, out int port) ? port : 21; // Default to 21 if parsing fails
            Logger.Debug("Settings have been set.");
        }

        public void LoadSettings()
        {
            FtpSettings = SettingsFileManager.LoadSettings<FtpSettings>(FtpSettingsFilePath);

            if (UploadSettings == null) UploadSettings = new ObservableCollection<SettingsControlViewModel>();

            foreach (var setting in FtpSettings.UploadSettings)
            {
                var scView = new Views.uc_SettingsItem();
                var scv = new SettingsControlViewModel
                {
                    UploadEnabled = setting.IsUploadEnabled,
                    LocalPath = setting.LocalPath,
                    SoftwareName = setting.Software
                };
                scView.DataContext = scv;
                UploadSettings.Add(scv);
            }
            RegisteredName = FtpSettings.RegisteredName;

            // FTP settings can be used like this
            FtpServerAddress = FtpSettings.ServerAddress;
            FtpUsername = FtpSettings.Username;
            FtpPassword = FtpSettings.Password;
            FtpPort = FtpSettings.Port.ToString();
        }

        private void AddSetting()
        {
            UploadSettings.Add(new SettingsControlViewModel());
        }

        public void RemoveSetting()
        {
            if (UploadSettings.Any())
            {
                UploadSettings.RemoveAt(UploadSettings.Count - 1);
            }
        }

        public bool CanAddSetting()
        {
            return UploadSettings.Count < MAXIMUM_UPLOAD_SETTINGS;
        }

        public bool CanRemoveSetting()
        {
            return UploadSettings.Any();
        }

        private void ClearSettings()
        {
            Properties.Settings.Default.Reset();
            Properties.Settings.Default.Save();
            Logger.Debug("Settings reset.");
        }

        #endregion
    }
}
