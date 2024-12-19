using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TaskManagement.Helpers; // Пространство имен для FieldValidator
using TaskManagement.Models;
using TaskManagement.TaskManagementDataSetTableAdapters;
using TaskManagement.ViewModels;
using System.Configuration;

namespace TaskManagement.MVVM.ViewModels.AdministratorPageViewModel
{
    public class StatusEditViewModel : ViewModelBase
    {
        private TaskManagementDataSet _dataSet;
        private StatusesTableAdapter _statusesTableAdapter;
        private readonly UserActivityLogger _userActivityLogger;
        private string _currentUserLogin; // Логин текущего пользователя

        private int _statusId;
        private string _statusName;

        public int StatusId
        {
            get => _statusId;
            private set
            {
                _statusId = value;
                OnPropertyChanged();
            }
        }

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

        public event EventHandler StatusUpdated;

        public string CurrentUserLogin
        {
            get => _currentUserLogin;
            set
            {
                _currentUserLogin = value;
                OnPropertyChanged();
            }
        }

        public StatusEditViewModel(int statusId, string currentUserLogin)
        {
            _dataSet = new TaskManagementDataSet();
            _statusesTableAdapter = new StatusesTableAdapter();
            _userActivityLogger = new UserActivityLogger(ConfigurationManager.ConnectionStrings["TaskManagement.Properties.Settings.TaskManagementConnectionString"].ConnectionString);

            StatusId = statusId;
            CurrentUserLogin = currentUserLogin;

            LoadStatus(statusId);

            SaveCommand = new RelayCommand(SaveStatus);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void LoadStatus(int statusId)
        {
            _statusesTableAdapter.Fill(_dataSet.Statuses);
            var statusRow = _dataSet.Statuses.FindByID(statusId);

            if (statusRow != null)
            {
                StatusName = statusRow.Name;
            }
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

                var existingStatus = _dataSet.Statuses.FirstOrDefault(s => s.Name == StatusName && s.ID != StatusId);
                if (existingStatus != null)
                {
                    MessageBox.Show("Статус с таким именем уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var statusRow = _dataSet.Statuses.FindByID(StatusId);

                if (statusRow != null)
                {
                    string oldStatusName = statusRow.Name;

                    statusRow.Name = StatusName;
                    _statusesTableAdapter.Update(_dataSet.Statuses);

                    MessageBox.Show("Статус успешно обновлен!");

                    // Логирование действия
                    _userActivityLogger.LogUserActivity(GetUserId(CurrentUserLogin), CurrentUserLogin, $"обновил статус с именем: {oldStatusName} на {StatusName}");

                    StatusUpdated?.Invoke(this, EventArgs.Empty);

                    Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this)?.Close();
                }
                else
                {
                    MessageBox.Show("Статус не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении статуса: {ex.Message}");
            }
        }

        private void Cancel(object parameter)
        {
            // Логирование отмены
            _userActivityLogger.LogUserActivity(GetUserId(CurrentUserLogin), CurrentUserLogin, "Отмена редактирования статуса");

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
