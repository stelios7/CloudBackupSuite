using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloud_Backup_Core.Models.Settings
{
    public class FtpSettings
    {
        public bool UseDefaultFtpCredentials { get; set; }

        public string ServerAddress { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }

        public List<UploadSetting> UploadSettings { get; set; } = new();
        public string RegisteredName { get; set; }

    }
}
