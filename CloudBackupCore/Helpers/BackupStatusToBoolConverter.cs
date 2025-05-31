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
    internal class BackupStatusToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is MainViewModel.BACKUP_STATUS status)
            {
                switch (status)
                {
                    case MainViewModel.BACKUP_STATUS.UPLOADING:
                        return false;
                    default:
                        return true;
                }

            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
