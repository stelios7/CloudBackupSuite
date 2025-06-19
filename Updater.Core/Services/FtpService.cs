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

        public async Task<UpdateManifest> DownloadManifestAsync(string manifestPath)
        {
            string json = await DownloadStringAsync($"{_config.Host}/{manifestPath}");
            return JsonSerializer.Deserialize<UpdateManifest>(json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }

        public async Task DownloadFileAsync(string remoteFile, string localFile)
        {
            string url = $"{_config.Host}/{_config.RemoteFilesPath}/{remoteFile}";
            var request  = (FtpWebRequest)WebRequest.Create(url);
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.Credentials = new NetworkCredential(_config.Username, _config.Password);

            using var response = (FtpWebResponse)await request.GetResponseAsync();
            using var stream = response.GetResponseStream();
            using var fileStream = new FileStream(localFile, FileMode.Create, FileAccess.ReadWrite);
            await stream.CopyToAsync(fileStream);
        }

        private async Task<string> DownloadStringAsync(string url)
        {
            var request = (FtpWebRequest)WebRequest.Create(url);
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.Credentials = new NetworkCredential(_config.Username, _config.Password);

            using var response = (FtpWebResponse)await request.GetResponseAsync();
            using var stream = response.GetResponseStream();
            using var reader = new StreamReader(stream, Encoding.UTF8);
            return await reader.ReadToEndAsync();
        }
    }
}
