using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Cloud_Backup_Core.Helpers
{
    public static class SettingsFileManager
    {
        public static T LoadSettings<T>(string filePath) where T : new()
        {
            if (!File.Exists(filePath))
            {
                var defaultInstance = new T();
                SaveSettings(filePath, defaultInstance); // Create default instance if file does not exist
                return defaultInstance;
            }

            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<T>(json) ?? new T(); // Deserialize JSON to object, return new instance if deserialization fails
        }

        public static void SaveSettings<T>(string filePath, T settings)
        {
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory) && !string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory); // Ensure the directory exists
            DirectoryInfo di = new DirectoryInfo(directory ?? string.Empty);
            di.Attributes |= FileAttributes.Directory | FileAttributes.Hidden; // Set directory attributes to hidden

            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            WriteFile(new FileInfo(filePath), json);
        }

        private static void WriteFile(FileInfo f, string content)
        {
            f.Attributes &= ~FileAttributes.ReadOnly; // Remove ReadOnly attribute if it exists
            f.Attributes &= ~FileAttributes.Hidden; // Remove Hidden attribute if it exists
            File.WriteAllText(f.FullName, content); // Create or overwrite the file with an empty string
            f.Attributes |= FileAttributes.ReadOnly | FileAttributes.Hidden; // Set file attributes to read-only and hidden
        }
    }
}
