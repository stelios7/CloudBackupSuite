using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Cloud_Backup_Core.Helpers;
using Cloud_Backup_Core.Viewmodels;

namespace Cloud_Backup_Core.Viewmodels
{
    internal class SettingsFtpViewModel : BaseViewModel
    {
        private string softwareName;

        public string SoftwareName
        {
            get { return softwareName; }
            set { softwareName = value; 
                OnPropertyChanged(nameof(SoftwareName));
            }
        }

        private string Sfuel = "Sfuel";
        private string Lpg = "LpgRetail";
        private string Softruck = "Softruck";
        private string Upsales = "Upsales";

        private RelayCommand saveCommand;
        public RelayCommand SaveCommand
        {
            get { return saveCommand ?? (saveCommand = new RelayCommand(execute => { SetSettings(); SaveSettings(); }, canExecute => true)); }
        }

        public ObservableCollection<SettingsControlViewModel> UploadSettings { get; set; }

        public SettingsFtpViewModel()
        {
            LoadSettings();
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.Save();
            Logger.Debug("Settings saved.");
        }

        private void SetSettings()
        {
            var sfuel = UploadSettings.FirstOrDefault(a => a.SoftwareName == Sfuel);
            var lpg = UploadSettings.FirstOrDefault(a => a.SoftwareName == Lpg);
            var softruck = UploadSettings.FirstOrDefault(a => a.SoftwareName == Softruck);
            var upsales = UploadSettings.FirstOrDefault(a => a.SoftwareName == Upsales);

            Properties.Settings.Default.UploadSfuel = sfuel.UploadEnabled;
            Properties.Settings.Default.UploadLpg = lpg.UploadEnabled;
            Properties.Settings.Default.UploadSoftruck = softruck.UploadEnabled;
            Properties.Settings.Default.UploadUpsales = upsales.UploadEnabled;

            Properties.Settings.Default.SfuelLocalFilepath = sfuel.LocalPath;
            Properties.Settings.Default.LpgLocalFilePath = lpg.LocalPath;
            Properties.Settings.Default.SoftruckLocalFilePath = softruck.LocalPath;
            Properties.Settings.Default.UpsalesLocalFilePath = upsales.LocalPath;

            Properties.Settings.Default.SoftwareName = this.SoftwareName;
        }

        private void LoadSettings()
        {
            Properties.Settings settings = Properties.Settings.Default;
            UploadSettings = new ObservableCollection<SettingsControlViewModel>()
            {
                new SettingsControlViewModel { UploadEnabled = settings.UploadSfuel, LocalPath = settings.SfuelLocalFilepath, SoftwareName = Sfuel },
                new SettingsControlViewModel { UploadEnabled = settings.UploadLpg, LocalPath = settings.LpgLocalFilePath, SoftwareName = Lpg },
                new SettingsControlViewModel { UploadEnabled = settings.UploadSoftruck, LocalPath = settings.SoftruckLocalFilePath, SoftwareName = Softruck },
                new SettingsControlViewModel { UploadEnabled = settings.UploadUpsales, LocalPath = settings.UpsalesLocalFilePath, SoftwareName = Upsales }
            };
            SoftwareName = settings.SoftwareName;

            Logger.Debug("Settings loaded.");
        }

        private void ClearSettings()
        {
            Properties.Settings.Default.Reset();
            Properties.Settings.Default.Save();
            Logger.Debug("Settings reset.");
        }
    }
}
