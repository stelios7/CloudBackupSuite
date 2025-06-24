using Cloud_Backup_Core.Helpers;
using Cloud_Backup_Core.Helpers.Settings;
using Cloud_Backup_Core.Models.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Cloud_Backup_Core.Viewmodels
{
    class SettingsLocalViewModel : BaseViewModel, IDataErrorInfo
    {
        #region PROPERTIES

        private bool _deleteLocalAfterUpload;
        public bool DeleteLocalAfterUpload
        {
            get => _deleteLocalAfterUpload;
            set
            {
                if (_deleteLocalAfterUpload != value)
                {
                    _deleteLocalAfterUpload = value;
                    OnPropertyChanged(nameof(DeleteLocalAfterUpload));
                }
            }
        }

        private bool _keepLocalBackup;
        public bool KeepLocalBackup
        {
            get => _keepLocalBackup;
            set
            {
                if (_keepLocalBackup != value)
                {
                    _keepLocalBackup = value;
                    OnPropertyChanged(nameof(KeepLocalBackup));
                }
            }
        }

        private int _maxUploadSizeMB;
        public int MaxUploadSizeMB
        {
            get => _maxUploadSizeMB;
            set
            {
                if (_maxUploadSizeMB != value)
                {
                    _maxUploadSizeMB = value;
                    OnPropertyChanged(nameof(MaxUploadSizeMB));
                }
            }
        }

        private bool _notifyOnFailure;
        public bool NotifyOnFailure
        {
            get => _notifyOnFailure;
            set
            {
                if (_notifyOnFailure != value)
                {
                    _notifyOnFailure = value;
                    OnPropertyChanged(nameof(NotifyOnFailure));
                }
            }
        }

        private bool _showLogs;
        public bool ShowLogs
        {
            get => _showLogs;
            set
            {
                if (_showLogs != value)
                {
                    _showLogs = value;
                    OnPropertyChanged(nameof(ShowLogs));
                }
            }
        }

        private bool _startWithWindows;
        public bool StartWithWindows
        {
            get => _startWithWindows;
            set
            {
                if (_startWithWindows != value)
                {
                    _startWithWindows = value;
                    OnPropertyChanged(nameof(StartWithWindows));
                }
            }
        }

        private string SettingsFilePath { get; }

        #endregion

        #region RELAY COMMANDS

        public RelayCommand SaveCommand => new RelayCommand(execute => SaveSettings(), canExecute => !HasErrors);

        #endregion

        public SettingsLocalViewModel()
        {
            SettingsFilePath = Path.Combine(AppContext.BaseDirectory, "settings", "local_settings.json");
            LoadSettings();
        }

        public void SaveSettings()
        {
            SetSettings();
            SettingsFileManager.SaveSettings<LocalSettings>(LocalSettingsFilePath, LocalSettings);
        }

        private void SetSettings()
        {
            LocalSettings.DeleteLocalFileAfterUpload = DeleteLocalAfterUpload;
            LocalSettings.KeepLocalBackup = KeepLocalBackup;
            LocalSettings.MaximumUploadSizeMB = MaxUploadSizeMB;
            LocalSettings.NotifyOnFailedUpload = NotifyOnFailure;
            LocalSettings.ShowLogs = ShowLogs;
            LocalSettings.StartWithWindows = StartWithWindows;
        }

        private readonly string LocalSettingsFilePath = Path.Combine(AppContext.BaseDirectory, "Settings", "local_settings.json");

        public static LocalSettings LocalSettings { get; private set; }

        public void LoadSettings()
        {
            LocalSettings = SettingsFileManager.LoadSettings<LocalSettings>(LocalSettingsFilePath) ?? new LocalSettings();

            DeleteLocalAfterUpload = LocalSettings.DeleteLocalFileAfterUpload;
            KeepLocalBackup = LocalSettings.KeepLocalBackup;
            MaxUploadSizeMB = LocalSettings.MaximumUploadSizeMB;
            NotifyOnFailure = LocalSettings.NotifyOnFailedUpload;
            ShowLogs = LocalSettings.ShowLogs;
            StartWithWindows = LocalSettings.StartWithWindows;
        }

        public string Error => null;

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(MaxUploadSizeMB):
                        if (MaxUploadSizeMB < 0)
                            return "Το όριο μεγέθους πρέπει να είναι μεγαλύτερο από 0. (Βάλε 0 για να αγνοηθεί)";
                        break;
                }
                return string.Empty;
            }
        }


        public bool HasErrors =>
            !string.IsNullOrEmpty(this[nameof(MaxUploadSizeMB)]);
    }
}
