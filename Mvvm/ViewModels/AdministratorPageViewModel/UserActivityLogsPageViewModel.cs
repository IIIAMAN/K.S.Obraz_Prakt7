using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TaskManagement.Helpers;
using TaskManagement.Models;
using TaskManagement.TaskManagementDataSetTableAdapters;
using TaskManagement.ViewModels;
using TaskManagement.MVVM.Views.AdministratorPages.UserActivityLogPage;
using System.Configuration;

namespace TaskManagement.MVVM.ViewModels.AdministratorPageViewModel
{
    public class UserActivityLogsPageViewModel : ViewModelBase
    {
        private TaskManagementDataSet _dataSet;
        private UserActivityLogTableAdapter _userActivityLogTableAdapter;
        private readonly UserActivityLogger _userActivityLogger;
        private ObservableCollection<TaskManagementDataSet.UserActivityLogRow> _userActivityLogs;
        private TaskManagementDataSet.UserActivityLogRow _selectedUserActivityLog;
        private string _currentUserLogin;

        public ObservableCollection<TaskManagementDataSet.UserActivityLogRow> UserActivityLogs
        {
            get => _userActivityLogs;
            set
            {
                _userActivityLogs = value;
                OnPropertyChanged();
            }
        }

        public TaskManagementDataSet.UserActivityLogRow SelectedUserActivityLog
        {
            get => _selectedUserActivityLog;
            set
            {
                _selectedUserActivityLog = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddLogCommand { get; }
        public ICommand EditLogCommand { get; }
        public ICommand DeleteLogCommand { get; }

        public string CurrentUserLogin
        {
            get => _currentUserLogin;
            set
            {
                _currentUserLogin = value;
                OnPropertyChanged();
            }
        }

        public UserActivityLogsPageViewModel(string currentUserLogin)
        {
            _dataSet = new TaskManagementDataSet();
            _userActivityLogTableAdapter = new UserActivityLogTableAdapter();
            _userActivityLogger = new UserActivityLogger(ConfigurationManager.ConnectionStrings["TaskManagement.Properties.Settings.TaskManagementConnectionString"].ConnectionString);
            _currentUserLogin = currentUserLogin;

            LoadUserActivityLogs();

            AddLogCommand = new RelayCommand(AddLog);
            EditLogCommand = new RelayCommand(EditLog, CanEditOrDeleteLog);
            DeleteLogCommand = new RelayCommand(DeleteLog, CanEditOrDeleteLog);
        }

        public void LoadUserActivityLogs()
        {
            _userActivityLogTableAdapter.Fill(_dataSet.UserActivityLog);
            UserActivityLogs = new ObservableCollection<TaskManagementDataSet.UserActivityLogRow>(_dataSet.UserActivityLog.Cast<TaskManagementDataSet.UserActivityLogRow>());
        }

        private void AddLog(object parameter)
        {
            var addLogWindow = new UserActivityLogAddWindow(CurrentUserLogin);
            var viewModel = (UserActivityLogAddViewModel)addLogWindow.DataContext;

            viewModel.LogAdded += (s, args) =>
            {
                LoadUserActivityLogs();
                _userActivityLogger.LogUserActivity(GetUserId(_currentUserLogin), _currentUserLogin, $"добавил новую запись в лог активности: {viewModel.Action}");
            };

            addLogWindow.ShowDialog();
        }

        private void EditLog(object parameter)
        {
            if (SelectedUserActivityLog != null)
            {
                var editWindow = new UserActivityLogEditWindow(SelectedUserActivityLog.ID, CurrentUserLogin); // Передаем ID записи
                var viewModel = (UserActivityLogEditViewModel)editWindow.DataContext;

                viewModel.LogUpdated += (s, args) =>
                {
                    LoadUserActivityLogs();
                    _userActivityLogger.LogUserActivity(GetUserId(_currentUserLogin), _currentUserLogin, $"обновил запись активности ID {SelectedUserActivityLog.ID}");
                };

                if (editWindow.ShowDialog() == true)
                {
                    LoadUserActivityLogs(); // Обновляем DataGrid после редактирования
                }
            }
        }

        private void DeleteLog(object parameter)
        {
            if (SelectedUserActivityLog != null)
            {
                if (MessageBox.Show("Вы уверены, что хотите удалить эту запись?", "Удаление записи", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        var logRow = _dataSet.UserActivityLog.FindByID(SelectedUserActivityLog.ID);
                        if (logRow != null)
                        {
                            logRow.Delete();
                            _userActivityLogTableAdapter.Update(_dataSet.UserActivityLog);

                            // Логирование успешного удаления записи
                            _userActivityLogger.LogUserActivity(GetUserId(_currentUserLogin), _currentUserLogin, $"удалил запись активности");

                            LoadUserActivityLogs(); // Обновляем данные после удаления
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении записи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private bool CanEditOrDeleteLog(object parameter)
        {
            return SelectedUserActivityLog != null;
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
