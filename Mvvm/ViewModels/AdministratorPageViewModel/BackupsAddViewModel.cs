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
    public class BackupsAddViewModel : ViewModelBase
    {
        private TaskManagementDataSet _dataSet;
        private BackupsTableAdapter _backupsTableAdapter;
        private readonly UserActivityLogger _userActivityLogger;
        private string _currentUserLogin;

        private DateTime _backupDate;
        private string _backupFilePath;

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

        public event EventHandler BackupAdded;

        public BackupsAddViewModel(string currentUserLogin)
        {
            _dataSet = new TaskManagementDataSet();
            _backupsTableAdapter = new BackupsTableAdapter();
            _userActivityLogger = new UserActivityLogger(ConfigurationManager.ConnectionStrings["TaskManagement.Properties.Settings.TaskManagementConnectionString"].ConnectionString);
            _currentUserLogin = currentUserLogin;

            SaveCommand = new RelayCommand(SaveBackup);
            CancelCommand = new RelayCommand(Cancel);
        }

        public void SetDefaultValues()
        {
            BackupDate = DateTime.Now;
        }

        private void SaveBackup(object parameter)
        {
            try
            {
                // Проверка на заполнение всех полей
                if (string.IsNullOrEmpty(BackupFilePath))
                {
                    MessageBox.Show("Пожалуйста, заполните путь к файлу резервной копии.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Добавление новой резервной копии
                _backupsTableAdapter.Insert(BackupDate, BackupFilePath);

                // Логирование успешного добавления резервной копии
                _userActivityLogger.LogUserActivity(GetUserId(_currentUserLogin), _currentUserLogin, $"успешно добавил резервную копию с датой {BackupDate} и файлом {BackupFilePath}");

                MessageBox.Show("Резервная копия успешно добавлена!");

                BackupAdded?.Invoke(this, EventArgs.Empty);

                // Закрытие окна
                Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this)?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении резервной копии: {ex.Message}");
            }
        }

        private void Cancel(object parameter)
        {
            // Закрытие окна
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
