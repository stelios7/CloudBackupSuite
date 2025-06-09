using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloud_Backup_Core.Models.Settings
{
    public class BackupSettings
    {
        public string BackupDirectory { get; set; }
        public string SqlServerInstance { get; set; }
        public string SqlDatabaseName { get; set; }
        public string SqlUsername { get; set; }
        public string SqlPassword { get; set; }

        public string ScheduleBackup { get; set; }
    }
}
