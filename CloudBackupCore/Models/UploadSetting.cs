using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Cloud_Backup_Core.Helpers;

namespace Cloud_Backup_Core.Models
{
    public class UploadSetting : BaseSetting
    {
        public bool IsUploadEnabled { get; set; }
        public string SoftwareName { get; set; }
        public string LocalPath { get; set; }

        public UploadSetting(bool enb, string sName, string lp)
        {
            IsUploadEnabled = enb;
            SoftwareName = sName;
            LocalPath = lp;
        }
    }

    public class UserSettings
    {
        public string User { get; set; }
        public List<UploadSetting> UploadSettings { get; set; }

        public UserSettings(string user)
        {
            this.User = user;
            UploadSettings = new List<UploadSetting>();
        }
    }
}
