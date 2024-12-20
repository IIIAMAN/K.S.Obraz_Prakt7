using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TaskManagement.Helpers;
using TaskManagement.Models;
using TaskManagement.TaskManagementDataSetTableAdapters;
using TaskManagement.ViewModels;
using TaskManagement.MVVM.Views.AdministratorPages.TaskHistoryPage;
using System.Configuration;

namespace TaskManagement.MVVM.ViewModels.AdministratorPageViewModel
{
    public class TaskHistoryPageViewModel : ViewModelBase
    {
        private TaskManagementDataSet _dataSet;
        private TaskHistoryTableAdapter _taskHistoryTableAdapter;
        private readonly UserActivityLogger _userActivityLogger;
        private ObservableCollection<TaskManagementDataSet.TaskHistoryRow> _taskHistories;
        private TaskManagementDataSet.TaskHistoryRow _selectedTaskHistory;
        private string _currentUserLogin;

        public ObservableCollection<TaskManagementDataSet.TaskHistoryRow> TaskHistories
        {
            get => _taskHistories;
            set
            {
                _taskHistories = value;
                OnPropertyChanged();
            }
        }

        public TaskManagementDataSet.TaskHistoryRow SelectedTaskHistory
        {
            get => _selectedTaskHistory;
            set
            {
                _selectedTaskHistory = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddHistoryCommand { get; }
        public ICommand EditHistoryCommand { get; }
        public ICommand DeleteHistoryCommand { get; }

        public string CurrentUserLogin
        {
            get => _currentUserLogin;
            set
            {
                _currentUserLogin = value;
                OnPropertyChanged();
            }
        }

        public TaskHistoryPageViewModel(string currentUserLogin)
        {
            _dataSet = new TaskManagementDataSet();
            _taskHistoryTableAdapter = new TaskHistoryTableAdapter();
            _userActivityLogger = new UserActivityLogger(ConfigurationManager.ConnectionStrings["TaskManagement.Properties.Settings.TaskManagementConnectionString"].ConnectionString);
            _currentUserLogin = currentUserLogin;

            LoadTaskHistories();

            AddHistoryCommand = new RelayCommand(AddHistory);
            EditHistoryCommand = new RelayCommand(EditHistory, CanEditOrDeleteHistory);
            DeleteHistoryCommand = new RelayCommand(DeleteHistory, CanEditOrDeleteHistory);
        }

        public void LoadTaskHistories()
        {
            _taskHistoryTableAdapter.Fill(_dataSet.TaskHistory);
            TaskHistories = new ObservableCollection<TaskManagementDataSet.TaskHistoryRow>(_dataSet.TaskHistory.Cast<TaskManagementDataSet.TaskHistoryRow>());
        }

        private void AddHistory(object parameter)
        {
            var addHistoryWindow = new TaskHistoryAddWindow(CurrentUserLogin);
            var viewModel = (TaskHistoryAddViewModel)addHistoryWindow.DataContext;

            viewModel.HistoryAdded += (s, args) =>
            {
                LoadTaskHistories();
                _userActivityLogger.LogUserActivity(GetUserId(_currentUserLogin), _currentUserLogin, $"добавил новую запись в историю задачи ID {viewModel.SelectedTaskId}");
            };

            addHistoryWindow.ShowDialog();
        }

        private void EditHistory(object parameter)
        {
            if (SelectedTaskHistory != null)
            {
                var editWindow = new TaskHistoryEditWindow(SelectedTaskHistory.ID, CurrentUserLogin);
                var viewModel = (TaskHistoryEditViewModel)editWindow.DataContext;

                viewModel.HistoryUpdated += (s, args) =>
                {
                    LoadTaskHistories();
                    _userActivityLogger.LogUserActivity(GetUserId(_currentUserLogin), _currentUserLogin, $"обновил запись в истории задачи ID {viewModel.SelectedTaskId}");
                };

                if (editWindow.ShowDialog() == true)
                {
                    LoadTaskHistories();
                }
            }
        }

        private void DeleteHistory(object parameter)
        {
            if (SelectedTaskHistory != null)
            {
                if (MessageBox.Show("Вы уверены, что хотите удалить эту запись?", "Удаление записи", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        var historyRow = _dataSet.TaskHistory.FindByID(SelectedTaskHistory.ID);
                        if (historyRow != null)
                        {
                            historyRow.Delete();
                            _taskHistoryTableAdapter.Update(_dataSet.TaskHistory);

                            _userActivityLogger.LogUserActivity(GetUserId(_currentUserLogin), _currentUserLogin, $"удалил запись в истории задачи ID {SelectedTaskHistory.ID}");

                            LoadTaskHistories();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении записи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private bool CanEditOrDeleteHistory(object parameter)
        {
            return SelectedTaskHistory != null;
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
