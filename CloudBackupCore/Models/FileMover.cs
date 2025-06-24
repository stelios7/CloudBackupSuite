using Cloud_Backup_Core.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloud_Backup_Core.Models
{
    public class FileMover : BaseSetting
    {
        #region SINGLETON

        private static FileMover _instance;
        private static readonly object _lock = new object();

        public static FileMover Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new FileMover();
                    }
                    return _instance;
                }
            }
        }

        #endregion

        // Files older than this will be deleted
        private const int MAXIMUM_DAYS_TO_KEEP = 7;

        public void MoveFile(string sourcePath)
        {
            try
            {
                string parent = Directory.GetParent(sourcePath).Parent.FullName;

                string destination = Path.Combine(parent, Path.GetFileName(sourcePath));
                File.Move(sourcePath, destination);

                Thread.Sleep(200);

                DeleteOldFiles(parent);
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
        }

        private void DeleteOldFiles(string sourcePath)
        {
            try
            {
                var files = Directory.GetFiles(sourcePath).Where(s => File.GetLastAccessTime(s) < DateTime.Now.AddDays(-MAXIMUM_DAYS_TO_KEEP));

                foreach (var file in files)
                {
                    File.Delete(file);
                }

               Logger.Debug("Cleanup completed.");
            }
            catch (Exception ex)
            {
                Logger.Debug($"Error: {ex.Message}");
            }
        }
    }
}
