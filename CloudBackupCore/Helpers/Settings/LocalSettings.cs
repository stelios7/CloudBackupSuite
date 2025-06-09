using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloud_Backup_Core.Helpers.Settings
{
    class LocalSettings
    {
        public bool ShowLogs { get; set; } = true;
        public bool StartWithWindows { get; set; } = false;
        public bool DeleteLocalFileAfterUpload { get; set; } = false;
        public bool NotifyOnFailedUpload { get; set; } = false;
        public bool KeepLocalBackup { get; set; } = false;
        public int MaximumUploadSizeMB { get; set; } = 1000; // Default to 1000 MB
    }
}
