using System;
using System.Collections.ObjectModel;
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
    public class UserActivityLogEditViewModel : ViewModelBase
    {
        private TaskManagementDataSet _dataSet;
        private UserActivityLogTableAdapter _userActivityLogTableAdapter;
        private UsersTableAdapter _usersTableAdapter;
        private readonly UserActivityLogger _userActivityLogger;
        private string _currentUserLogin;

        private int _logId;
        private int _selectedUserId;
        private string _action;
        private DateTime _timestamp;

        public int LogId
        {
            get => _logId;
            private set
            {
                _logId = value;
                OnPropertyChanged();
            }
        }

        public int SelectedUserId
        {
            get => _selectedUserId;
            set
            {
                _selectedUserId = value;
                OnPropertyChanged();
            }
        }

        public string Action
        {
            get => _action;
            set
            {
                _action = value;
                OnPropertyChanged();
            }
        }

        public DateTime Timestamp
        {
            get => _timestamp;
            set
            {
                _timestamp = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<TaskManagementDataSet.UsersRow> Users { get; set; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler LogUpdated;

        public UserActivityLogEditViewModel(int logId, string currentUserLogin)
        {
            _dataSet = new TaskManagementDataSet();
            _userActivityLogTableAdapter = new UserActivityLogTableAdapter();
            _usersTableAdapter = new UsersTableAdapter();
            _userActivityLogger = new UserActivityLogger(ConfigurationManager.ConnectionStrings["TaskManagement.Properties.Settings.TaskManagementConnectionString"].ConnectionString);

            LogId = logId;
            _currentUserLogin = currentUserLogin;

            LoadUserActivityLogData(logId);
            LoadComboBoxData();

            SaveCommand = new RelayCommand(SaveLog);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void LoadUserActivityLogData(int logId)
        {
            _userActivityLogTableAdapter.Fill(_dataSet.UserActivityLog);
            var logRow = _dataSet.UserActivityLog.FindByID(logId);

            if (logRow != null)
            {
                SelectedUserId = logRow.UserID;
                Action = logRow.Action;
                Timestamp = logRow.Timestamp;
            }
        }

        public void LoadComboBoxData()
        {
            _usersTableAdapter.Fill(_dataSet.Users);
            Users = new ObservableCollection<TaskManagementDataSet.UsersRow>(_dataSet.Users.Cast<TaskManagementDataSet.UsersRow>());
        }

        private void SaveLog(object parameter)
        {
            try
            {
                if (SelectedUserId == 0 || string.IsNullOrEmpty(Action))
                {
                    MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var logRow = _dataSet.UserActivityLog.FindByID(LogId);

                if (logRow != null)
                {
                    logRow.UserID = SelectedUserId;
                    logRow.Action = Action;

                    _userActivityLogTableAdapter.Update(logRow);

                    MessageBox.Show("Запись успешно обновлена!");

                    _userActivityLogger.LogUserActivity(GetUserId(_currentUserLogin), _currentUserLogin, $"обновил запись активности пользователя ID {SelectedUserId}: {Action}");

                    LogUpdated?.Invoke(this, EventArgs.Empty);

                    Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this)?.Close();
                }
                else
                {
                    MessageBox.Show("Запись не найдена.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении записи: {ex.Message}");
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
