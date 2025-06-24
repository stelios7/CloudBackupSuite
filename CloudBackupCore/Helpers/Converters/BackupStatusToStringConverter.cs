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
    public class BackupStatusToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is MainViewModel.STATE_STATUS status)
            {
                switch (status)
                {
                    case MainViewModel.STATE_STATUS.IDLE:
                        return "Idle";
                    case MainViewModel.STATE_STATUS.ONLINE:
                        return "Online";
                    case MainViewModel.STATE_STATUS.UPLOADING:
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
