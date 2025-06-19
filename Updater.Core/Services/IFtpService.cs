using Updater.Core.Models;

namespace Updater.Core.Services
{
    public interface IFtpService
    {
        Task<UpdateManifest> DownloadManifestAsync(string manifestPath);
        Task DownloadFileAsync(string remoteFile, string localFile);
    }
}
