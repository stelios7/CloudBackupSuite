using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Updater.Core.Models;


namespace Updater.Core.Services
{
    public class FtpService : IFtpService
    {
        private readonly FtpConfig _config;
        public FtpService(FtpConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        // Downloads the manifest JSON from a remote URL and saves it locally.
        // Returns the parsed UpdateManifest instance if successful.
        public async Task<UpdateManifest> DownloadManifestAsync(string manifestRelativePath)
        {
            try
            {
                // Combine the FTP host and relative path to build the full URL
                string manifestUrl = $"{_config.Host}/{manifestRelativePath}";

                // Download the JSON content as string
                string json = await DownloadStringAsync(manifestUrl);

                // Write the JSON to local manifest.json and return the deserialized object
                return JsonSerializer.Deserialize<UpdateManifest>(json,
                   new JsonSerializerOptions
                   {
                       PropertyNameCaseInsensitive = true,
                   }
                   );
            }
            catch (Exception ex)
            {
                Logger.Error($"Error downloading manifest: {ex.Message}");
                return null;
            }
        }
        /// <summary>
        /// 
        /// Downloads a file from the FTP server to the local file system.
        public async Task<bool> DownloadFileAsync(string remoteFile, string localFile)
        {
            try
            {

                string url = $"{_config.Host}/{_config.RemoteFilesPath}/{remoteFile}";

                var request = (FtpWebRequest)WebRequest.Create(url);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential(_config.Username, _config.Password);
                request.UseBinary = true;
                request.UsePassive = true;
                request.KeepAlive = false;

                using var response = (FtpWebResponse)await request.GetResponseAsync();
                using var responseStream = response.GetResponseStream();

                long totalBytes = response.ContentLength;
                long downloadedBytes = 0;

                byte[] buffer = new byte[8192];
                int bytesRead;

                using var fileStream = new FileStream(localFile, FileMode.Create, FileAccess.Write, FileShare.None);

                Logger.Log($"Starting download: {remoteFile} ({totalBytes} bytes)");

                while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                    downloadedBytes += bytesRead;

                    if (totalBytes > 0)
                    {
                        double percent = (double)downloadedBytes / totalBytes * 100;
                        Logger.Log($"Downloading {remoteFile}: {percent:F2}%");
                    }
                    else
                    {
                        Logger.DownloadUpdate($"Downloading {remoteFile}: {downloadedBytes / 1024} kbytes downloaded...");
                    }
                }

                Logger.Log($"Download completed: {remoteFile}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"{ex.Message}");
                return false;
            }
        }

        private async Task<string> DownloadStringAsync(string url)
        {
            try
            {
                var request = (FtpWebRequest)WebRequest.Create(url);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential(_config.Username, _config.Password);

                using var response = (FtpWebResponse)await request.GetResponseAsync();
                using var stream = response.GetResponseStream();
                using var reader = new StreamReader(stream, Encoding.UTF8);
                return await reader.ReadToEndAsync();
            }
            catch (WebException ex)
            {
                Logger.Error($"Failed to download string from {url}: {ex.Message}");
                return string.Empty;
            }
            catch (Exception ex)
            {
                Logger.Error($"An error occurred while downloading string from {url}: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
