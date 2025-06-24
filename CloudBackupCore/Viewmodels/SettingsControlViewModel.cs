using Cloud_Backup_Core.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloud_Backup_Core.Viewmodels
{
    internal class SettingsControlViewModel : BaseViewModel
    {
        public RelayCommand OpenFolderBrowser_command => new RelayCommand(execute => OpenFolderBrowser(), canExecute => true);

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

        private string _softwareName;
        public string SoftwareName
        {
            get { return _softwareName ?? "Input Software"; }
            set
            {
                _softwareName = value;
                OnPropertyChanged(nameof(SoftwareName));
            }
        }
    }
}
