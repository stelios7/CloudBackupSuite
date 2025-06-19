using System.Diagnostics;
using System.IO;
using System.IO.Compression;
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
            var manifest = await _ftp.DownloadManifestAsync(_config.Ftp.RemoteManifestPath);
            if (!Version.TryParse(_config.CurrentVersion, out var currentVer)) return;
            if (!Version.TryParse(manifest.LatestVersion, out var latestVer)) return;

            if (latestVer <= currentVer)
            {
                Console.WriteLine("No updates available.");
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
                Console.WriteLine($"{ex.Message}");
            }
            finally
            {
                // Check for Backup Before Update
                if (_config.BackupBeforeUpdate)
                {
                    string backupFolder = Path.Combine(AppContext.BaseDirectory, "backup", DateTime.Now.ToString());
                    Directory.CreateDirectory(backupFolder);

                    foreach (var file in Directory.GetFiles(AppContext.BaseDirectory, "*", SearchOption.AllDirectories))
                    {
                        string relPath = Path.GetRelativePath(AppContext.BaseDirectory, file);
                        string destPath = Path.Combine(backupFolder, relPath);
                        Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                        File.Copy(file, destPath, true);
                    }
                }


                // Extract the zip file to the application directory
                ZipFile.ExtractToDirectory(zipPath, AppContext.BaseDirectory, overwriteFiles: true);
                File.Delete(zipPath);


                // Restart application
                Process.Start(_config.ExecutablePath);
                Environment.Exit(0);
            }
        }

    }

}
