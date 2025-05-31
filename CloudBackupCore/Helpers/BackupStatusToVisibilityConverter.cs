using Cloud_Backup_Core.Viewmodels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Cloud_Backup_Core.Helpers
{
    internal class BackupStatusToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is MainViewModel.BACKUP_STATUS status)
            {
                switch (status)
                {
                    case MainViewModel.BACKUP_STATUS.UPLOADING:
                        return Visibility.Hidden;
                    default:
                        return Visibility.Visible;
                }

            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
