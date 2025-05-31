using Cloud_Backup_Core.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        private int _autoBackupInterval;
        public int AutoBackupInterval
        {
            get => _autoBackupInterval;
            set
            {
                if (_autoBackupInterval != value)
                {
                    _autoBackupInterval = value;
                    OnPropertyChanged(nameof(AutoBackupInterval));
                }
            }
        }

        private bool _isAutoBackupEnabled;
        public bool IsAutoBackupEnabled
        {
            get => _isAutoBackupEnabled;
            set
            {
                if (_isAutoBackupEnabled != value)
                {
                    _isAutoBackupEnabled = value;
                    OnPropertyChanged(nameof(IsAutoBackupEnabled));
                }
            }
        }

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

        #endregion


        public RelayCommand SaveCommand { get; }
        private string SettingsFilePath { get; }

        public SettingsLocalViewModel()
        {
            SaveCommand = new RelayCommand(execute => SaveSettings(), canExecute => HasErrors);
            SettingsFilePath = "C:/Program Files/CloudBackupCore/settings";
            LoadSettings();
        }

        public void SaveSettings()
        {
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(SettingsFilePath, json);
        }

        public void LoadSettings()
        {
            if (!File.Exists(SettingsFilePath))
                return;

            var json = File.ReadAllText(SettingsFilePath);
            var loaded = JsonSerializer.Deserialize<SettingsLocalViewModel>(json);

            if (loaded != null)
            {
                AutoBackupInterval = loaded.AutoBackupInterval;
                IsAutoBackupEnabled = loaded.IsAutoBackupEnabled;
                DeleteLocalAfterUpload = loaded.DeleteLocalAfterUpload;
                KeepLocalBackup = loaded.KeepLocalBackup;
                MaxUploadSizeMB = loaded.MaxUploadSizeMB;
                NotifyOnFailure = loaded.NotifyOnFailure;
                ShowLogs = loaded.ShowLogs;
                StartWithWindows = loaded.StartWithWindows;
            }
        }

        public string Error => null;

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(AutoBackupInterval):
                        if (AutoBackupInterval <= 0)
                            return "Το χρονικό διάστημα πρέπει να είναι μεγαλύτερο του μηδενός.";
                        break;

                    case nameof(MaxUploadSizeMB):
                        if (MaxUploadSizeMB < 0)
                            return "Το όριο μεγέθους πρέπει να είναι μεγαλύτερο από 0. (Βάλε 0 για να αγνοηθεί)";
                        break;
                }
                return string.Empty;
            }
        }


        public bool HasErrors =>
            !string.IsNullOrEmpty(this[nameof(AutoBackupInterval)]) ||
            !string.IsNullOrEmpty(this[nameof(MaxUploadSizeMB)]);
    }
}
