using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using Cloud_Backup_Core.Helpers;
using Cloud_Backup_Core.Models;
using Quartz.Impl.Matchers;
using System.Diagnostics;

namespace Cloud_Backup_Core.Viewmodels
{
    internal class SettingsBackupViewModel : BaseViewModel
    {
        private readonly SqlBackupService _backupService;
        private string _username;
        private string _password;

        public string SqlServerName
        {
            get { return _sqlServerName; }
            set
            {
                _sqlServerName = value;
                OnPropertyChanged(nameof(SqlServerName));
            }
        }
        public string ScheduledTime
        {
            get { return _scheduledTime; }
            set { _scheduledTime = value; 
                OnPropertyChanged(nameof(ScheduledTime));
            }
        }
        public string SqlUsername
        {
            get { return _username; }
            set { _username = value; 
                OnPropertyChanged(nameof(SqlUsername));
            }
        }
        public string SqlPassword
        {
            get { return _password; }
            set { _password = value; 
                OnPropertyChanged(nameof(SqlPassword));
            }
        }

        private IScheduler _scheduler;
        private BackupModel _backupModel;
        private string _statusMessage;
        private bool _isBackingUp;
        private string _sqlServerName;
        private string _scheduledTime = "10:00";

        public RelayCommand BackupCommand { get; }
        public RelayCommand BrowseCommand { get; }
        public RelayCommand SaveBackupSettingsCommand { get; }
        public RelayCommand SetScheduleCommand { get; }

        public SettingsBackupViewModel()
        {
            _backupModel = new BackupModel();
            BackupCommand = new RelayCommand(execute => ExecuteBackup(), canExecute => CanExecuteBackup());
            BrowseCommand = new RelayCommand(execute => BrowseBackupFolder(), canExecute => true);
            SetScheduleCommand = new RelayCommand(execute => SetScheduleForBackup(), canExecute => true);
            SaveBackupSettingsCommand = new RelayCommand(execute => SaveBackupSettings(), canExecute => CanExecuteBackup());
            LoadSettings();

            var server = Properties.Settings.Default.SQLServerInstance;
            var db = Properties.Settings.Default.SQLDatabaseForBackup;
            var u = Properties.Settings.Default.SQLUsername;
            var p = Properties.Settings.Default.SQLPassword;
            string connectionString = @$"Server={server};Database={db};User Id={u};Password={p};TrustServerCertificate=True";
            _backupService = new SqlBackupService(connectionString);

            InitializeScheduler();
        }

        private async void InitializeScheduler()
        {
            _scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await _scheduler.Start();

            // Reschedule the job if a schedule exists
            if (!string.IsNullOrWhiteSpace(ScheduledTime))
            {
                SetScheduleForBackup();
            }
        }

        private void SaveBackupSettings()
        {
            Properties.Settings.Default.SQLDatabaseBackupPath = BackupFolder;
            Properties.Settings.Default.SQLServerInstance = SqlServerName;
            Properties.Settings.Default.SQLDatabaseForBackup = DatabaseName;

            //TODO: set and save schedule backup timer → DONE
            Properties.Settings.Default.SQLDatabaseBackupScheduleTime = TimeSpan.Parse(ScheduledTime);

            Properties.Settings.Default.SQLUsername = SqlUsername;
            Properties.Settings.Default.SQLPassword = SqlPassword;

            Properties.Settings.Default.Save();
            SetScheduleForBackup();
        }

        private void LoadSettings()
        {
            var settings = Properties.Settings.Default;
            BackupFolder = settings.SQLDatabaseBackupPath;
            DatabaseName = settings.SQLDatabaseForBackup;
            SqlServerName = settings.SQLServerInstance;
            SqlUsername = settings.SQLUsername;
            SqlPassword = settings.SQLPassword; 
            ScheduledTime = settings.SQLDatabaseBackupScheduleTime.ToString(@"hh\:mm");
        }

        private async void SetScheduleForBackup()
        {
            await _scheduler.Clear(); // Clear existing jobs before rescheduling

            if (TimeSpan.TryParse(ScheduledTime, out TimeSpan scheduleTime))
            {
                var job = JobBuilder.Create<BackupJob>()
                    .WithIdentity("DailyBackupJob")
                    .Build();

                job.JobDataMap["BackupService"] = _backupService;
                job.JobDataMap["DatabaseName"] = DatabaseName;
                job.JobDataMap["BackupFolder"] = BackupFolder;

                var _trig = TriggerBuilder.Create()
                    .WithIdentity("Daily")
                    .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(scheduleTime.Hours, scheduleTime.Minutes))
                    .Build();

                var trigger = TriggerBuilder.Create()
                    .WithIdentity("BackupTrigger")
                    .WithSchedule(CronScheduleBuilder.CronSchedule("0 0/2 * * * ?"))
                    .Build();

                await _scheduler.ScheduleJob(job, _trig);
                StatusMessage = $"Backup scheduled at {ScheduledTime} daily.";

                var jobKeys = await _scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
                Debug.WriteLine($"Jobs scheduled: {string.Join(", ", jobKeys)}");

                var triggerKeys = await _scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup());
                Debug.WriteLine($"Triggers scheduled: {string.Join(", ", triggerKeys)}");
            }
            else
            {
                StatusMessage = "Invalid time format!";
            }
        }

        public string DatabaseName
        {
            get => _backupModel.DatabaseName;
            set
            {
                _backupModel.DatabaseName = value;
                OnPropertyChanged(nameof(DatabaseName));
            }
        }

        public string BackupFolder
        {
            get => _backupModel.BackupFolder;
            set
            {
                _backupModel.BackupFolder = value; OnPropertyChanged(nameof(BackupFolder));
            }
        }


        public String StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        public bool IsBackingUp
        {
            get => _isBackingUp;
            set
            {
                _isBackingUp = value;
                OnPropertyChanged(nameof(_isBackingUp));
            }
        }

        private void BrowseBackupFolder()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                BackupFolder = dialog.SelectedPath;
            }
        }

        private async void ExecuteBackup()
        {
            IsBackingUp = true;
            StatusMessage = "Backing up...";

            bool success = await _backupService.BackupDatabaseAsync(DatabaseName, BackupFolder);

            StatusMessage = success ? "Backup completed successfully!" : "Backup failed!";
            IsBackingUp = false;
        }

        private bool CanExecuteBackup()
        {
            return !IsBackingUp && !string.IsNullOrWhiteSpace(DatabaseName) && !string.IsNullOrWhiteSpace(BackupFolder) && !string.IsNullOrWhiteSpace(SqlServerName);
        }
    }
}
