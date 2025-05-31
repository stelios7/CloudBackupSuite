using Cloud_Backup_Core.Models;
using Newtonsoft.Json;
using System.IO;

namespace Cloud_Backup_Core.Helpers
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

        private static Config _instance;
        private static readonly object _lock = new object();

        public static Config Instance
        {
            get
            {
                lock (_lock)
                {
                    _instance ??= new Config();
                    return _instance;
                }
            }
        }

        public void Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Configuration file not found: {filePath}");
            }

            string json = File.ReadAllText(filePath);
            var loadedConfig = JsonConvert.DeserializeObject<Config>(json);

            if (loadedConfig == null)
                throw new InvalidOperationException("Failed to deserialize configuration file.");

            // Αντιγραφή των ιδιοτήτων στο singleton instance
            lock (_lock)
            {
                _instance = loadedConfig;
            }
        }
    }
}
