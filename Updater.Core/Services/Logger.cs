using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Updater.Core.Services
{
    public static class Logger
    {
        private static readonly string LogPath = Path.Combine(AppContext.BaseDirectory, "updater.log");
        private const long MaxLogSizeBytes = 10 * 1024 * 1024; // 10MB

        public static void DownloadUpdate(string message)
        {
            var newLogPath = Path.Combine(AppContext.BaseDirectory, "updater_new_update.log");
            try
            {
                // TODO: Clear or rotate log file if it exceeds 10MB
                if (File.Exists(newLogPath))
                {
                    var fileInfo = new FileInfo(newLogPath);
                    if (fileInfo.Length > MaxLogSizeBytes)
                    {
                        // Option 1: Clear the log
                        File.WriteAllText(newLogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Log cleared due to size > 10MB\n");

                        // Option 2: Rename (rotate) and start new file
                        // string archiveName = $"updater_{DateTime.Now:yyyyMMdd_HHmmss}.log";
                        // File.Move(LogPath, Path.Combine(AppContext.BaseDirectory, archiveName));
                    }
                }

                string entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss} DL] {message}\n";
                File.AppendAllText(newLogPath, entry);
            }
            catch
            {
                // Silent fail
            }
        }

        public static void Log(string message)
        {
            try
            {
                // TODO: Clear or rotate log file if it exceeds 10MB
                if (File.Exists(LogPath))
                {
                    var fileInfo = new FileInfo(LogPath);
                    if (fileInfo.Length > MaxLogSizeBytes)
                    {
                        // Option 1: Clear the log
                        File.WriteAllText(LogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Log cleared due to size > 10MB\n");

                        // Option 2: Rename (rotate) and start new file
                        // string archiveName = $"updater_{DateTime.Now:yyyyMMdd_HHmmss}.log";
                        // File.Move(LogPath, Path.Combine(AppContext.BaseDirectory, archiveName));
                    }
                }

                string entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n";
                File.AppendAllText(LogPath, entry);
            }
            catch
            {
                // Silent fail
            }
        }

        public static void Error(string message)
        {
            try
            {
                // TODO: Clear or rotate log file if it exceeds 10MB
                if (File.Exists(LogPath))
                {
                    var fileInfo = new FileInfo(LogPath);
                    if (fileInfo.Length > MaxLogSizeBytes)
                    {
                        // Option 1: Clear the log
                        File.WriteAllText(LogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Log cleared due to size > 10MB\n");

                        // Option 2: Rename (rotate) and start new file
                        // string archiveName = $"updater_{DateTime.Now:yyyyMMdd_HHmmss}.log";
                        // File.Move(LogPath, Path.Combine(AppContext.BaseDirectory, archiveName));
                    }
                }

                string entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss} ERROR] {message}\n";
                File.AppendAllText(LogPath, entry);
            }
            catch
            {
                // Silent fail
            }
        }
    }

}
