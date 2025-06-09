using SettingsManager = SettingsFileManager.SettingsFileManager;
using System.Net;
using System.Dynamic;

namespace CloudUpdater
{
    internal class Program
    {
        private static string UPDATER_SETTINGS_FILE = "updater_config.json";
        private static string PROGRAM_NAME = "Cloud_Backup_Core";
        static void Main(string[] args)
        {
            Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "update", "temp"));

            DirectoryInfo settingsDir = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings"));
            Directory.CreateDirectory(Path.Combine(settingsDir.FullName, "settings"));
            settingsDir.Attributes |= FileAttributes.Directory | FileAttributes.Hidden;

            try
            {
                string programSettingsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings");

                // Load configuration settings from JSON file
                Config config = SettingsManager.LoadSettings<Config>(Path.Combine(programSettingsDirectory, UPDATER_SETTINGS_FILE));

                //Config config = Config.Load(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), PROGRAM_NAME, "updater_config.json")); 

                string localVersionPath = Path.Combine(programSettingsDirectory, config.CurrentVersionFile);
                string remoteVersionUrl = $"ftp://{config.FtpUsername}@{config.FtpServer.Replace("ftp://", "")}/{config.RemoteUpdatePath}/{config.CurrentVersionFile}"; 
                //ftp://stelios_updater@iad1-shared-b8-15.dreamhost.com/cloud_backup_core/updates/version.txt 

                string localVersion = File.Exists(localVersionPath) ? File.ReadAllText(localVersionPath).Trim() : "0.0.0";
                string remoteVersion = DownloadVersionFile(remoteVersionUrl, config.FtpUsername, config.FtpPassword);

                Console.WriteLine($"Local Version: {localVersion}");
                Console.WriteLine($"Remote Version: {remoteVersion}");

                if (CompareVersions(remoteVersion, localVersion))
                {
#if DEBUG
                    Console.WriteLine($"Update available: {remoteVersion}. Proceed? [y/n]");
                    if (Console.ReadLine().ToUpper() == "Y")
                    {
                        DownloadUpdate(config);
                    }
                    else
                    {
                        Console.WriteLine("Aborting.");
                        return;
                    }
#else
                  DownloadUpdate(config);
#endif
                }
                else
                {
                    Console.WriteLine("No updates found.");
                }
                File.WriteAllText(localVersionPath, remoteVersion);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static bool CompareVersions(string remoteVersion, string localVersion)
        {
            string[] remoteParts = remoteVersion.Split('.');
            string[] localParts = localVersion.Split('.');
            if (remoteParts.Length != 3 || localParts.Length != 3)
            {
                throw new ArgumentException("Version strings must be in the format 'major.minor.patch'");
            }
            if (!int.TryParse(remoteParts[0], out int major) || !int.TryParse(localParts[0], out int localMajor) ||
                !int.TryParse(remoteParts[1], out int minor) || !int.TryParse(localParts[1], out int localMinor) ||
                !int.TryParse(remoteParts[2], out int patch) || !int.TryParse(localParts[2], out int localPatch))
            {
                throw new ArgumentException("Version strings must be numeric");
            }

            return (major, minor, patch) switch
            {
                _ when major > localMajor => true,
                _ when minor > localMinor => true,
                _ when patch > localPatch => true,
                _ => false
            };
        }

        /// <summary>
        /// Downloads the version.txt file from the FTP server and returns the version string.
        /// </summary>
        static string DownloadVersionFile(string url, string user, string pass)
        {
            try
            {
                using WebClient client = new WebClient();
                client.Credentials = new NetworkCredential(user, pass);
                return client.DownloadString(url).Trim();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading version file: {ex.Message}");
                return "0.0.0"; // If there's an error, assume no update is available
            }
        }

        /// <summary>
        /// Downloads and applies the update if a new version is found.
        /// </summary>
        static void DownloadUpdate(Config config)
        {
            try
            {
                string updateZip = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "update", "temp", "update.zip");
                string updateUrl = $"ftp://{config.FtpUsername}@{config.FtpServer.Replace("ftp://", "")}/{config.RemoteUpdatePath}/update.zip";

                //using WebClient client = new WebClient();
                //client.Credentials = new NetworkCredential(config.FtpUsername, config.FtpPassword);
                //client.DownloadFile(updateUrl, updateZip);

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(updateUrl);
                request.Credentials = new NetworkCredential(config.FtpUsername, config.FtpPassword);
                request.Method = WebRequestMethods.Ftp.DownloadFile;

                using (Stream ftpStream = request.GetResponse().GetResponseStream())
                using (Stream fileStream = File.Create(updateZip))
                {
                    ftpStream.CopyTo(fileStream);
                }

                Console.WriteLine("Download complete. Extracting update...");
                try
                {
                    var process = System.Diagnostics.Process.GetProcessesByName(
                        Path.GetFileNameWithoutExtension(config.ExecutableName)).FirstOrDefault();

                    process?.Kill(); // Kill the old process if running
                    System.Threading.Thread.Sleep(2000); // Wait for app to close

                    // Extract downloaded zip file to install path
                    System.IO.Compression.ZipFile.ExtractToDirectory(updateZip, AppDomain.CurrentDomain.BaseDirectory, true);
                    File.Delete(updateZip);

                    // Restart application
                    System.Diagnostics.Process.Start(Path.Combine(config.LocalAppPath, config.ExecutableName));
                    Console.WriteLine("Application restarted.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error restarting application: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during update: {ex.Message}");
            }
        }
    }
}
