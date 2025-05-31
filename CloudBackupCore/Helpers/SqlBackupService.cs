using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace Cloud_Backup_Core.Helpers
{
    internal class SqlBackupService
    {
        private readonly string _connectionString;

        public SqlBackupService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<bool> BackupDatabaseAsync(string databaseName, string backupFolder)
        {
            try
            {
                if (!Directory.Exists(backupFolder))
                    Directory.CreateDirectory(backupFolder);

                string query = "SELECT SUSER_NAME();";
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        string user = (string)await cmd.ExecuteScalarAsync();
                        Debug.WriteLine($"Connected as: {user}");
                    }
                }


                string backupFile = Path.Combine(backupFolder, $"{databaseName}_{DateTime.Now:yyyyMMdd_HHmmss}.bak");
                string backupQuery = $@"
BACKUP DATABASE [{databaseName}]
TO DISK = '{backupFile}'
WITH FORMAT, INIT, STATS = 10;";

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(backupQuery, conn))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                return true;
            }

            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                return false;
            }
        }
    }
}
