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
    public class UserActivityLogAddViewModel : ViewModelBase
    {
        private TaskManagementDataSet _dataSet;
        private UserActivityLogTableAdapter _userActivityLogTableAdapter;
        private UsersTableAdapter _usersTableAdapter;
        private readonly UserActivityLogger _userActivityLogger;

        private int _selectedUserId;
        private string _action;
        private DateTime _timestamp;

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

        public event EventHandler LogAdded;

        public UserActivityLogAddViewModel(string currentUserLogin)
        {
            _dataSet = new TaskManagementDataSet();
            _userActivityLogTableAdapter = new UserActivityLogTableAdapter();
            _usersTableAdapter = new UsersTableAdapter();
            _userActivityLogger = new UserActivityLogger(ConfigurationManager.ConnectionStrings["TaskManagement.Properties.Settings.TaskManagementConnectionString"].ConnectionString);

            SaveCommand = new RelayCommand(SaveLog);
            CancelCommand = new RelayCommand(Cancel);
        }

        public void LoadComboBoxData()
        {
            _usersTableAdapter.Fill(_dataSet.Users);
            Users = new ObservableCollection<TaskManagementDataSet.UsersRow>(_dataSet.Users.Cast<TaskManagementDataSet.UsersRow>());
        }

        public void SetDefaultValues()
        {
            Timestamp = DateTime.Now;
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

                _userActivityLogTableAdapter.Insert(SelectedUserId, Action, Timestamp);

                MessageBox.Show("Запись успешно добавлена!");

                // Логирование успешного добавления записи
                _userActivityLogger.LogUserActivity(GetUserId(SelectedUserId), SelectedUserId.ToString(), $"добавил запись активности: {Action}");

                LogAdded?.Invoke(this, EventArgs.Empty);

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

        private int GetUserId(int userId)
        {
            var userRow = GetUserFromDatabase(userId);
            return userRow?.ID ?? 0;
        }

        private TaskManagementDataSet.UsersRow GetUserFromDatabase(int userId)
        {
            TaskManagementDataSet.UsersDataTable usersTable = new TaskManagementDataSet.UsersDataTable();
            UsersTableAdapter usersAdapter = new UsersTableAdapter();
            usersAdapter.Fill(usersTable);

            var userRows = usersTable.Select($"ID = {userId}");
            return userRows.Length > 0 ? (TaskManagementDataSet.UsersRow)userRows[0] : null;
        }
    }
}
