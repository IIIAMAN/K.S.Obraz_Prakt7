using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TaskManagement.Helpers;
using TaskManagement.Models;
using TaskManagement.TaskManagementDataSetTableAdapters;
using TaskManagement.ViewModels;
using System.Configuration;

namespace TaskManagement.MVVM.ViewModels.AdministratorPageViewModel
{
    public class BackupsEditViewModel : ViewModelBase
    {
        private TaskManagementDataSet _dataSet;
        private BackupsTableAdapter _backupsTableAdapter;
        private readonly UserActivityLogger _userActivityLogger;
        private string _currentUserLogin;

        private int _backupId;
        private DateTime _backupDate;
        private string _backupFilePath;

        public int BackupId
        {
            get => _backupId;
            private set
            {
                _backupId = value;
                OnPropertyChanged();
            }
        }

        public DateTime BackupDate
        {
            get => _backupDate;
            set
            {
                _backupDate = value;
                OnPropertyChanged();
            }
        }

        public string BackupFilePath
        {
            get => _backupFilePath;
            set
            {
                _backupFilePath = value;
                OnPropertyChanged();
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler BackupUpdated;

        public BackupsEditViewModel(int backupId, string currentUserLogin)
        {
            _dataSet = new TaskManagementDataSet();
            _backupsTableAdapter = new BackupsTableAdapter();
            _userActivityLogger = new UserActivityLogger(ConfigurationManager.ConnectionStrings["TaskManagement.Properties.Settings.TaskManagementConnectionString"].ConnectionString);
            _currentUserLogin = currentUserLogin;

            BackupId = backupId;

            LoadBackupData(backupId);

            SaveCommand = new RelayCommand(SaveBackup);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void LoadBackupData(int backupId)
        {
            _backupsTableAdapter.Fill(_dataSet.Backups);
            var backupRow = _dataSet.Backups.FindByID(backupId);

            if (backupRow != null)
            {
                BackupDate = backupRow.BackupDate;
                BackupFilePath = backupRow.BackupFilePath;
            }
        }

        private void SaveBackup(object parameter)
        {
            try
            {
                if (string.IsNullOrEmpty(BackupFilePath))
                {
                    MessageBox.Show("Пожалуйста, заполните путь к файлу резервной копии.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var backupRow = _dataSet.Backups.FindByID(BackupId);

                if (backupRow != null)
                {
                    backupRow.BackupFilePath = BackupFilePath;

                    _backupsTableAdapter.Update(backupRow);

                    _userActivityLogger.LogUserActivity(GetUserId(_currentUserLogin), _currentUserLogin, $"успешно обновил резервную копию с ID {BackupId} и файлом {BackupFilePath}");

                    MessageBox.Show("Резервная копия успешно обновлена!");

                    BackupUpdated?.Invoke(this, EventArgs.Empty);

                    Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this)?.Close();
                }
                else
                {
                    MessageBox.Show("Резервная копия не найдена.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении резервной копии: {ex.Message}");
            }
        }

        private void Cancel(object parameter)
        {
            Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this)?.Close();
        }

        private int GetUserId(string login)
        {
            var userRow = GetUserFromDatabase(login);
            return userRow?.ID ?? 0;
        }

        private TaskManagementDataSet.UsersRow GetUserFromDatabase(string login)
        {
            TaskManagementDataSet.UsersDataTable usersTable = new TaskManagementDataSet.UsersDataTable();
            UsersTableAdapter usersAdapter = new UsersTableAdapter();
            usersAdapter.Fill(usersTable);

            var userRows = usersTable.Select($"Login = '{login}'");
            return userRows.Length > 0 ? (TaskManagementDataSet.UsersRow)userRows[0] : null;
        }
    }
}
