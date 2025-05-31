using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloud_Backup_Core.Helpers;
using Quartz;

namespace Cloud_Backup_Core.Models
{
    class BackupJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            Debug.WriteLine("Executing backup cronjob");
            var backupService = (SqlBackupService)context.JobDetail.JobDataMap["BackupService"];
            string databaseName = (string)context.JobDetail.JobDataMap["DatabaseName"];
            string backupFolder = (string)context.JobDetail.JobDataMap["BackupFolder"];

            await backupService.BackupDatabaseAsync(databaseName, backupFolder);
        }
    }
}
