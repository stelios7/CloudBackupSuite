using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloud_Backup_Core.Helpers
{
    public static class Logger
    {
        private static readonly object _lock = new object();
        private static string _logDirectory = Path.Combine(AppContext.BaseDirectory, "Logs");

        static Logger()
        {
            if (!Directory.Exists(_logDirectory))
                Directory.CreateDirectory(_logDirectory);
        }

        public static void Info(string message) => WriteLog("INFO", message);
        public static void Warning(string message) => WriteLog("WARNING", message);
        public static void Error(string message) => WriteLog("ERROR", message);
        public static void Debug(string message) => WriteLog("DEBUG", message);

        private static void WriteLog(string level, string message)
        {
            lock (_lock)
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string logMessage = $"[{timestamp}] [{level}] {message}{Environment.NewLine}";

                string logFile = Path.Combine(_logDirectory, $"log_{DateTime.Now:yyyy-MM-dd}.txt");

                File.AppendAllText(logFile, logMessage, Encoding.UTF8);
            }
        }
    }
}
