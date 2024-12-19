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
    public class PriorityAddViewModel : ViewModelBase
    {
        private TaskManagementDataSet _dataSet;
        private PrioritiesTableAdapter _prioritiesTableAdapter;
        private readonly UserActivityLogger _userActivityLogger;
        private string _currentUserLogin;

        private string _priorityName;

        public PriorityAddViewModel()
        {
            _dataSet = new TaskManagementDataSet();
            _prioritiesTableAdapter = new PrioritiesTableAdapter();
            _userActivityLogger = new UserActivityLogger(ConfigurationManager.ConnectionStrings["TaskManagement.Properties.Settings.TaskManagementConnectionString"].ConnectionString);

            SaveCommand = new RelayCommand(SavePriority);
            CancelCommand = new RelayCommand(Cancel);
        }

        public string PriorityName
        {
            get => _priorityName;
            set
            {
                _priorityName = value;
                OnPropertyChanged();
            }
        }

        public string CurrentUserLogin
        {
            get => _currentUserLogin;
            set
            {
                _currentUserLogin = value;
                OnPropertyChanged();
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler PriorityAdded;

        private void SavePriority(object parameter)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(PriorityName))
                {
                    MessageBox.Show("Имя приоритета не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var existingPriority = _dataSet.Priorities.FirstOrDefault(p => p.Name == PriorityName);
                if (existingPriority != null)
                {
                    MessageBox.Show("Приоритет с таким именем уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var newPriorityRow = _dataSet.Priorities.NewPrioritiesRow();
                newPriorityRow.Name = PriorityName;

                _dataSet.Priorities.AddPrioritiesRow(newPriorityRow);
                _prioritiesTableAdapter.Update(_dataSet.Priorities);

                // Логирование успешного добавления приоритета
                _userActivityLogger.LogUserActivity(GetUserId(CurrentUserLogin), CurrentUserLogin, $"успешно добавил приоритет с именем {PriorityName}");

                MessageBox.Show("Приоритет успешно добавлен!");

                PriorityAdded?.Invoke(this, EventArgs.Empty);

                Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this)?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении приоритета: {ex.Message}");
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
