using System.Text.Json;
using System.Windows;
using Updater.Core;
using Updater.Core.Models;
using Updater.Core.Services;


namespace Updater.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                // Define the path to the configuration file
#if DEBUG
                string configPath = Path.Combine(AppContext.BaseDirectory, "settings", "core.json");
#else
                string configPath = Path.Combine(AppContext.BaseDirectory, "settings", "core.json");
#endif 

                if (!File.Exists(configPath))
                {
                    Logger.Log("Configuration file not found. Creating default configuration...");

                    // Δημιουργία φακέλου αν δεν υπάρχει
                    string settingsDir = Path.GetDirectoryName(configPath);
                    if (!Directory.Exists(settingsDir))
                        Directory.CreateDirectory(settingsDir);

                    // Serialize & save
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    string defaultJson = JsonSerializer.Serialize(GetDefaultConfig(), options);
                    File.WriteAllText(configPath, defaultJson);

                    Logger.Log("Default configuration created at: " + configPath);
                }

                // Read and deserialize the configuration file
                var json = await File.ReadAllTextAsync(configPath);
                var config = JsonSerializer.Deserialize<AppUpdateConfig>(json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                    }
                    );

                if (config == null)
                {
                    Logger.Error("Failed to deserialize configuration.");
                    return;
                }

                // Initialize the FTP service with the configuration
                IFtpService ftp = new FtpService(config.Ftp);


                // Create the updater engine with the configuration and FTP service
                var updater = new UpdaterEngine(config, ftp);
                await updater.RunAsync();

            }
            catch (Exception ex)
            {
                string logFile = Path.Combine(AppContext.BaseDirectory, "updater.log");
                await File.AppendAllTextAsync(logFile, $"{DateTime.Now}: {ex.Message}\n");
            }
        }

        private static AppUpdateConfig GetDefaultConfig()
        {
            // Default ρύθμιση
            var defaultConfig = new AppUpdateConfig
            {
                AppName = "MyApp",
                CurrentVersion = "1.0.0",
                ExecutablePath = "MyApp.exe",
                BackupBeforeUpdate = true,
                Ftp = new FtpConfig
                {
                    Host = "ftp://127.0.0.1:21",
                    Username = "user",
                    Password = "pass",
                    RemoteManifestPath = "app/manifest.json",
                    RemoteFilesPath = "app/"
                }
            };
            return defaultConfig;
        }
    }
}
