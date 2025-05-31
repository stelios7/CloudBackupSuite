using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloud_Backup_Core.Models
{
    internal class BackupModel
    {
        public string DatabaseName { get;set; }
        public string BackupFolder{ get; set; }
        public TimeSpan ScheduledTime { get; set; }
    }
}
