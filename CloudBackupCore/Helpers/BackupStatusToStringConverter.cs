using Cloud_Backup_Core.Viewmodels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Cloud_Backup_Core.Helpers
{
    internal class BackupStatusToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is MainViewModel.BACKUP_STATUS status)
            {
                switch (status)
                {
                    case MainViewModel.BACKUP_STATUS.IDLE:
                        return "Idle";
                    case MainViewModel.BACKUP_STATUS.ONLINE:
                        return "Online";
                    case MainViewModel.BACKUP_STATUS.UPLOADING:
                        return "Uploading";
                    default:
                        return "Error";
                }

            }
            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
