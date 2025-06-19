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
                string configPath = Path.Combine(@"C:\Users\User\Documents\stelios\Code\Cloud Backup Core\Updater.Client\bin\Debug\net9.0-windows\settings\core.json");
#else
                string configPath = Path.Combine(AppContext.BaseDirectory, "settings", "core.json");
#endif 

                if (!File.Exists(configPath))
                {
                    Console.WriteLine("Configuration file not found.");
                    return;
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
                    Console.WriteLine("Failed to deserialize configuration.");
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
    }
}
