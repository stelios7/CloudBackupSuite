using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using Cloud_Backup_Core.Helpers;
using Cloud_Backup_Core.Helpers.Settings;
using Cloud_Backup_Core.Models.Settings;
using FluentFTP;

namespace Cloud_Backup_Core.Models
{
    public class FtpUploader : BaseSetting
    {
        #region SINGLETON

        private static FtpUploader _instance;
        private static readonly object _lock = new object();

        public static FtpUploader Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new FtpUploader();
                    }
                    return _instance;
                }
            }
        }

        #endregion

        #region PROPERTIES

        private readonly string remoteDirectory;
        private static FtpSettings FtpSettings => SettingsFileManager.LoadSettings<FtpSettings>(MainWindow.SETTINGS_FTP_JSON);

        private string ups;
        public string UploadProgressString
        {
            get { return ups; }
            set
            {
                ups = value;
                OnPropertyChanged(nameof(UploadProgressString));
            }
        }

        private double progressValue;

        public double ProgressValue
        {
            get { return progressValue; }
            set
            {
                progressValue = value;
                OnPropertyChanged(nameof(ProgressValue));
            }
        }

        private double uploadProgress;
        public double UploadProgress
        {
            get { return uploadProgress; }
            set
            {
                uploadProgress = value;
                OnPropertyChanged(nameof(UploadProgress));
            }
        }

        private string FtpServer;
        private string FtpUsername;
        private string FtpPassword;
        private int FtpPort;

        #endregion

        #region CONSTRUCTOR

        public FtpUploader()
        {
            FtpServer = FtpSettings.ServerAddress;
            FtpUsername = FtpSettings.Username;
            FtpPassword = FtpSettings.Password;
            FtpPort = FtpSettings.Port;
        }

        #endregion

        #region FUNCTIONS

        public async Task UploadFileFtp(string file_to_upload, string remote_ftp_destination, CancellationToken token)
        {
            try
            {
                string localFilePath = file_to_upload ?? @"C:\Users\paokf\Documents\caesium-image-compressor-2.1.0-win.zip";
                string remoteFilePath = $"{remote_ftp_destination}/{Path.GetFileName(localFilePath)}";
                var isUploadCompleted = false;

                Progress<FtpProgress> progress = new Progress<FtpProgress>(p =>
                {
                    var lfp = Path.GetFileName(localFilePath);
                    if (p.Progress == 100 && !isUploadCompleted)
                    {
                        Logger.Debug($"{lfp} Uploaded");
                        isUploadCompleted = true;
                        p.Progress = 0;
                    }
                    else
                    {
                        Logger.Debug($"{lfp} progress: {p.Progress}");
                    }
                    ProgressValue = p.Progress;
                    UploadProgressString = $"{((int)ProgressValue)}%";
                });


                using var client = new AsyncFtpClient(FtpSettings.ServerAddress, FtpSettings.Username, FtpSettings.Password, FtpSettings.Port);

                await client.Connect(token);

                await client.UploadFile(localFilePath, remoteFilePath, FtpRemoteExists.Overwrite, true, FtpVerify.None, progress, token);

                await client.Disconnect(token);


            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message);
            }
        }

        public async Task ReadFTP()
        {
            try
            {
                using var client = new AsyncFtpClient(FtpSettings.ServerAddress, FtpSettings.Username, FtpSettings.Password, FtpSettings.Port);
                await client.Connect();

                foreach (var item in await client.GetListing("/CLOUDBACKUP"))
                {
                    //FtpDirectories.Add($"{item.Type} - {item.Name} | {item.FullName}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public async Task CreateFTP_Directory()
        {
            Debug.WriteLine("Creating");
            try
            {
                using (var client = new AsyncFtpClient(FtpServer, FtpUsername, FtpPassword, FtpPort))
                {
                    await client.Connect();

                    string remoteDirectory = @"/CLOUDBACKUP/Sfuel/Test";

                    bool success = await client.CreateDirectory(remoteDirectory, true);

                    if (success)
                    {
                        Debug.WriteLine($"Directory {remoteDirectory} created successfully");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        #endregion
    }
}
