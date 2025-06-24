using Cloud_Backup_Core.Models;
using Newtonsoft.Json;
using System.IO;

namespace Cloud_Backup_Core.Helpers.Settings
{
    public class Config
    {
        public string FtpServer { get; set; }
        public string FtpUsername { get; set; }
        public string FtpPassword { get; set; }
        public int FtpPort { get; set; }
        public string RemoteUpdatePath { get; set; }
        public string FtpRootDirectory { get; set; }
        public string LocalAppPath { get; set; }
        public string CurrentVersionFile { get; set; }
        public string ExecutableName { get; set; }
        public string LocalUpdatePath { get; set; }
    }
}
