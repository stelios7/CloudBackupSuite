using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Updater.Core.Models
{
    public class AppUpdateConfig
    {
        public string AppName { get; set; }
        public string CurrentVersion { get; set; }
        public string ExecutablePath { get; set; }
        public bool BackupBeforeUpdate { get; set; }
        public FtpConfig Ftp { get; set; }
    }

    public class FtpConfig
    {
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string RemoteManifestPath { get; set; }
        public string RemoteFilesPath { get; set; }
    }
}
