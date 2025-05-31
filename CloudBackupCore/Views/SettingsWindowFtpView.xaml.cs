using Cloud_Backup_Core.Helpers;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows;
using System.Windows.Threading;

namespace Cloud_Backup_Core.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindowFtpView.xaml
    /// </summary>
    public partial class SettingsWindowFtpView : Window    {
        private const int SETTINGS_VIEW_ALIVE_TIMER = 90;
        private const int SETTINGS_WINDOW_TIMER = 120;

        public SettingsWindowFtpView()
        {
            Debug.WriteLine("Initiating settings window");
            InitializeComponent();
            this.Title = "FTP Settings";
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(SETTINGS_VIEW_ALIVE_TIMER);
            timer.Tick += (s, args) =>
            {
                timer.Stop();
                this.Close();
            };

            timer.Start();
            this.DataContext = new Viewmodels.SettingsFtpViewModel();
            //this.Closing += SettingsWindowFtpView_closing;
        }

        private void SettingsWindowFtpView_closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Perform actions before the window closes
            // For example, prompt the user to save changes
            MessageBoxResult result = System.Windows.MessageBox.Show("Do you want to save changes?", "Confirmation", System.Windows.MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                // Save changes
            }
            else if (result == MessageBoxResult.No)
            {
                // Cancel the closing operation
                //e.Cancel = true;
            }
        }

    }
}
