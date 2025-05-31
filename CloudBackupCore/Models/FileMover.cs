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
            int daysOld = 7; // Files older than this will be deleted
            try
            {
                foreach (string file in Directory.GetFiles(sourcePath))
                {
                    FileInfo fileInfo = new FileInfo(file);

                    // Check if the file is older than the specified days
                    if (fileInfo.LastWriteTime < DateTime.Now.AddDays(-daysOld))
                    {
                        Logger.Debug($"Deleting: {fileInfo.FullName}");
                        fileInfo.Delete();
                    }
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
