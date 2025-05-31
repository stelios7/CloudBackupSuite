using Cloud_Backup_Core.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloud_Backup_Core.Viewmodels
{
    internal class SettingsControlViewModel : BaseViewModel
    {
        private RelayCommand ofb_command;

        public RelayCommand OpenFolderBrowser_command
        {
            get { return ofb_command ?? (ofb_command = new RelayCommand(execute => OpenFolderBrowser(), canExecute => true)); }
        }

        private void OpenFolderBrowser()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                LocalPath = dialog.SelectedPath;
            }
        }

        public bool UploadEnabled { get; set; }
        private string? localPath;
        public string? LocalPath
        {
            get { return localPath; }
            set
            {
                localPath = value;
                OnPropertyChanged(nameof(LocalPath));
            }
        }
        private string? software;
        public string SoftwareName
        {
            get { return software ?? "NULL"; }
            set
            {
                software = value;
                OnPropertyChanged();
            }
        }
    }
}
