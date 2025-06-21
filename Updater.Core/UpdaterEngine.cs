using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
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
            try
            {
                UpdateManifest manifest = await _ftp.DownloadManifestAsync(_config.Ftp.RemoteManifestPath);
                if (!Version.TryParse(_config.CurrentVersion, out var currentVer)) return;
                if (!Version.TryParse(manifest.LatestVersion, out var latestVer)) return;

                if (latestVer <= currentVer)
                {
                    Logger.Log("No updates available.");
                    return;
                }

                string manifestDir = Path.Combine(AppContext.BaseDirectory, "settings", "updater");
                string manifestPath = Path.Combine(manifestDir, "manifest.json");
                string manifestBackupDir = Path.Combine(manifestDir, "manifest_backup");
                string manifestBackupPath = Path.Combine(manifestBackupDir, $"manifest_{DateTime.Now:yyyyMMdd_HHmm}.json");
                var options = new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

                // Backup existing manifest if it exists
                if (File.Exists(manifestPath))
                {
                    if (!Directory.Exists(manifestBackupDir))
                    {
                        Directory.CreateDirectory(manifestBackupDir);
                    }
                    File.Copy(manifestPath, manifestBackupPath, overwrite: true);
                }

                // Save the downloaded manifest object to file with Windows-1253 encoding

                string jsonManifest = JsonSerializer.Serialize(manifest, options);
                await File.WriteAllTextAsync(manifestPath, jsonManifest, Encoding.UTF8);

                Logger.Log("Manifest file saved successfully.");

                // Download the update zip file
                string zipPath = Path.Combine(AppContext.BaseDirectory, manifest.ZipFileName ?? string.Empty);

                await _ftp.DownloadFileAsync(manifest.ZipFileName, zipPath);

                // Check for Backup Before Update
                if (_config.BackupBeforeUpdate)
                {
                    string backupFolder = Path.Combine(AppContext.BaseDirectory, "backup", DateTime.Now.ToString("ddMMyyyy"));
                    Logger.Log($"Creating backup at: {backupFolder}");
                    Directory.CreateDirectory(backupFolder);

                    foreach (var file in Directory.GetFiles(AppContext.BaseDirectory, "*", SearchOption.AllDirectories))
                    {
                        string relPath = Path.GetRelativePath(AppContext.BaseDirectory, file);
                        string destPath = Path.Combine(backupFolder, relPath);
                        Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                        File.Copy(file, destPath, true);
                    }

                    string backupZipPath = Path.Combine(AppContext.BaseDirectory, "backup", $"backup_{DateTime.Now:yyyyMMdd_HHmm}.zip");
                    ZipFile.CreateFromDirectory(backupFolder, backupZipPath, CompressionLevel.Optimal, true);
                    Directory.Delete(backupFolder, true);
                    Logger.Log("Backup completed.");
                }

                // Extract the zip file
                Logger.Log("Extracting update zip...");
                ZipFile.ExtractToDirectory(zipPath, AppContext.BaseDirectory, overwriteFiles: true);
                File.Delete(zipPath);
                Logger.Log("Extraction complete. Zip file deleted.");


                _config.CurrentVersion = manifest.LatestVersion;
                string configPath = Path.Combine(AppContext.BaseDirectory, "settings", "updater", "core.json");
                await using (var writer = new StreamWriter(configPath, false, Encoding.GetEncoding(1253)))
                {
                    await writer.WriteAsync(JsonSerializer.Serialize(_config, options));
                }

                // Restart application
                Logger.Log($"Starting updated application: {_config.ExecutablePath}");
                Process.Start(_config.ExecutablePath);
                Logger.Log("Updater exiting.");
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Logger.Error($"{ex.Message}");
            }
        }
    }

}
