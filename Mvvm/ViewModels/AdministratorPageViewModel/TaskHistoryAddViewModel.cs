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
    public class TaskHistoryAddViewModel : ViewModelBase
    {
        private TaskManagementDataSet _dataSet;
        private TaskHistoryTableAdapter _taskHistoryTableAdapter;
        private TasksTableAdapter _tasksTableAdapter;
        private StatusesTableAdapter _statusesTableAdapter;
        private UsersTableAdapter _usersTableAdapter;
        private readonly UserActivityLogger _userActivityLogger;
        private string _currentUserLogin;

        private DateTime _changeDate;
        private int _selectedTaskId;
        private int _selectedStatusId;
        private int _selectedUserId;
        private DateTime _createdAt;

        public DateTime ChangeDate
        {
            get => _changeDate;
            set
            {
                _changeDate = value;
                OnPropertyChanged();
            }
        }

        public int SelectedTaskId
        {
            get => _selectedTaskId;
            set
            {
                _selectedTaskId = value;
                OnPropertyChanged();
            }
        }

        public int SelectedStatusId
        {
            get => _selectedStatusId;
            set
            {
                _selectedStatusId = value;
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

        public DateTime CreatedAt
        {
            get => _createdAt;
            set
            {
                _createdAt = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<TaskManagementDataSet.TasksRow> Tasks { get; set; }
        public ObservableCollection<TaskManagementDataSet.StatusesRow> Statuses { get; set; }
        public ObservableCollection<TaskManagementDataSet.UsersRow> Users { get; set; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler HistoryAdded;

        public TaskHistoryAddViewModel(string currentUserLogin)
        {
            _dataSet = new TaskManagementDataSet();
            _taskHistoryTableAdapter = new TaskHistoryTableAdapter();
            _tasksTableAdapter = new TasksTableAdapter();
            _statusesTableAdapter = new StatusesTableAdapter();
            _usersTableAdapter = new UsersTableAdapter();
            _userActivityLogger = new UserActivityLogger(ConfigurationManager.ConnectionStrings["TaskManagement.Properties.Settings.TaskManagementConnectionString"].ConnectionString);
            _currentUserLogin = currentUserLogin;

            SaveCommand = new RelayCommand(SaveHistory);
            CancelCommand = new RelayCommand(Cancel);
        }

        public void LoadComboBoxData()
        {
            _tasksTableAdapter.Fill(_dataSet.Tasks);
            _statusesTableAdapter.Fill(_dataSet.Statuses);
            _usersTableAdapter.Fill(_dataSet.Users);

            Tasks = new ObservableCollection<TaskManagementDataSet.TasksRow>(_dataSet.Tasks.Cast<TaskManagementDataSet.TasksRow>());
            Statuses = new ObservableCollection<TaskManagementDataSet.StatusesRow>(_dataSet.Statuses.Cast<TaskManagementDataSet.StatusesRow>());
            Users = new ObservableCollection<TaskManagementDataSet.UsersRow>(_dataSet.Users.Cast<TaskManagementDataSet.UsersRow>());
        }

        public void SetDefaultValues()
        {
            DateTime now = DateTime.Now;
            ChangeDate = now;
            CreatedAt = now;
        }

        private void SaveHistory(object parameter)
        {
            try
            {
                if (SelectedTaskId == 0 || SelectedStatusId == 0 || SelectedUserId == 0)
                {
                    MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _taskHistoryTableAdapter.Insert(ChangeDate, SelectedTaskId, SelectedStatusId, SelectedUserId, CreatedAt);

                MessageBox.Show("Запись успешно добавлена!");

                // Логирование успешного добавления записи
                _userActivityLogger.LogUserActivity(GetUserId(_currentUserLogin), _currentUserLogin, $"добавил запись в историю задачи ID {SelectedTaskId}");

                HistoryAdded?.Invoke(this, EventArgs.Empty);

                Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this)?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении записи: {ex.Message}");
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
