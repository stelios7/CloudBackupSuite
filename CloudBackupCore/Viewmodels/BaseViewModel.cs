using Cloud_Backup_Core.Helpers;
using Cloud_Backup_Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Cloud_Backup_Core.Viewmodels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        private FileMover _fileMover;
        public FileMover FileMover
        {
            get { return _fileMover ?? new FileMover(); }
            set { _fileMover = value; 
                OnPropertyChanged(nameof(FileMover));
            }
        }

        private FtpUploader _ftpUploader;
        public FtpUploader FtpUploader { get => _ftpUploader ?? new FtpUploader(); 
            set {  _ftpUploader = value;
                OnPropertyChanged(nameof(FtpUploader));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
