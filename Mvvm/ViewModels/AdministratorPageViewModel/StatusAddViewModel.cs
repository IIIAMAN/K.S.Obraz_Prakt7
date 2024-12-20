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
    public class StatusAddViewModel : ViewModelBase
    {
        private TaskManagementDataSet _dataSet;
        private StatusesTableAdapter _statusesTableAdapter;
        private readonly UserActivityLogger _userActivityLogger;
        private string _statusName;
        private string _currentUserLogin;

        public string StatusName
        {
            get => _statusName;
            set
            {
                _statusName = value;
                OnPropertyChanged();
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler StatusAdded;

        public string CurrentUserLogin
        {
            get => _currentUserLogin;
            set
            {
                _currentUserLogin = value;
                OnPropertyChanged();
            }
        }

        public StatusAddViewModel(string currentUserLogin)
        {
            _dataSet = new TaskManagementDataSet();
            _statusesTableAdapter = new StatusesTableAdapter();
            _userActivityLogger = new UserActivityLogger(ConfigurationManager.ConnectionStrings["TaskManagement.Properties.Settings.TaskManagementConnectionString"].ConnectionString);
            CurrentUserLogin = currentUserLogin;

            SaveCommand = new RelayCommand(SaveStatus);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void SaveStatus(object parameter)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(StatusName))
                {
                    MessageBox.Show("Имя статуса не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var existingStatus = _dataSet.Statuses.FirstOrDefault(s => s.Name == StatusName);
                if (existingStatus != null)
                {
                    MessageBox.Show("Статус с таким именем уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var newStatusRow = _dataSet.Statuses.NewStatusesRow();
                newStatusRow.Name = StatusName;

                _dataSet.Statuses.AddStatusesRow(newStatusRow);
                _statusesTableAdapter.Update(_dataSet.Statuses);

                MessageBox.Show("Статус успешно добавлен!");

                // Логирование действия
                _userActivityLogger.LogUserActivity(GetUserId(CurrentUserLogin), CurrentUserLogin, $"Добавление нового статуса: {StatusName}");

                StatusAdded?.Invoke(this, EventArgs.Empty);

                Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this)?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении статуса: {ex.Message}");
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
