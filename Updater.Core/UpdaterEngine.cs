using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Windows;
using Updater.Core.Models;
using Updater.Core.Services;

namespace Updater.Core
{
    public class UpdaterEngine
    {
        private readonly AppUpdateConfig _config;
        private readonly IFtpService _ftp;

        public UpdaterEngine(AppUpdateConfig config, IFtpService ftpService)
        {
            _config = config;
            _ftp = ftpService;
        }

        public async Task RunAsync()
        {
            UpdateManifest manifest = await _ftp.DownloadManifestAsync(_config.Ftp.RemoteManifestPath);
            if (!Version.TryParse(_config.CurrentVersion, out var currentVer)) return;
            if (!Version.TryParse(manifest.LatestVersion, out var latestVer)) return;

            if (latestVer <= currentVer)
            {
                Logger.Log("No updates available.");
                return;
            }

            // Download the update zip file
            string zipPath = Path.Combine(AppContext.BaseDirectory, manifest.ZipFileName ?? string.Empty);

            try
            {
                await _ftp.DownloadFileAsync(manifest.ZipFileName, zipPath);
            }
            catch (Exception ex)
            {
                Logger.Error($"{ex.Message}");
            }
            finally
            {
                // Check for Backup Before Update
                if (_config.BackupBeforeUpdate)
                {
                    string backupFolder = Path.Combine(AppContext.BaseDirectory, "backup", DateTime.Now.ToString());
                    Logger.Log($"Creating backup at: {backupFolder}");
                    Directory.CreateDirectory(backupFolder);

                    foreach (var file in Directory.GetFiles(AppContext.BaseDirectory, "*", SearchOption.AllDirectories))
                    {
                        string relPath = Path.GetRelativePath(AppContext.BaseDirectory, file);
                        string destPath = Path.Combine(backupFolder, relPath);
                        Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                        File.Copy(file, destPath, true);
                    }
                    Logger.Log("Backup completed.");
                }


                string manifestDir = Path.Combine(AppContext.BaseDirectory, "settings");
                string manifestPath = Path.Combine(manifestDir, "manifest.json");
                string manifestBackupPath = Path.Combine(manifestDir, $"manifest_{DateTime.Now.ToString("ddMMyyyy_HHmm")}.json");

                Directory.CreateDirectory(manifestDir);

                Logger.Log("Downloading manifest file...");

                await _ftp.DownloadFileAsync(_config.Ftp.RemoteManifestPath, manifestPath);

                Logger.Log("Manifest file downloaded successfully.");

                // Extract the zip file
                Logger.Log("Extracting update zip...");
                ZipFile.ExtractToDirectory(zipPath, AppContext.BaseDirectory, overwriteFiles: true);
                File.Delete(zipPath);
                Logger.Log("Extraction complete. Zip file deleted.");


                // Restart application
                Logger.Log($"Starting updated application: {_config.ExecutablePath}");
                Process.Start(_config.ExecutablePath);
                Logger.Log("Updater exiting.");
                Environment.Exit(0);
            }
        }

    }

}
