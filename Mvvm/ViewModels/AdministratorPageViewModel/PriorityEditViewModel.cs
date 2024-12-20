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
    public class PriorityEditViewModel : ViewModelBase
    {
        private TaskManagementDataSet _dataSet;
        private PrioritiesTableAdapter _prioritiesTableAdapter;
        private readonly UserActivityLogger _userActivityLogger;
        private string _currentUserLogin;

        private int _priorityId;
        private string _priorityName;

        public int PriorityId
        {
            get => _priorityId;
            private set
            {
                _priorityId = value;
                OnPropertyChanged();
            }
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

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler PriorityUpdated;

        public PriorityEditViewModel(int priorityId, string currentUserLogin)
        {
            _dataSet = new TaskManagementDataSet();
            _prioritiesTableAdapter = new PrioritiesTableAdapter();
            _userActivityLogger = new UserActivityLogger(ConfigurationManager.ConnectionStrings["TaskManagement.Properties.Settings.TaskManagementConnectionString"].ConnectionString);
            _currentUserLogin = currentUserLogin;

            PriorityId = priorityId;

            LoadPriority(priorityId);

            SaveCommand = new RelayCommand(SavePriority);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void LoadPriority(int priorityId)
        {
            _prioritiesTableAdapter.Fill(_dataSet.Priorities);
            var priorityRow = _dataSet.Priorities.FindByID(priorityId);

            if (priorityRow != null)
            {
                PriorityName = priorityRow.Name;
            }
        }

        private void SavePriority(object parameter)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(PriorityName))
                {
                    MessageBox.Show("Имя приоритета не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var existingPriority = _dataSet.Priorities.FirstOrDefault(p => p.Name == PriorityName && p.ID != PriorityId);
                if (existingPriority != null)
                {
                    MessageBox.Show("Приоритет с таким именем уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var priorityRow = _dataSet.Priorities.FindByID(PriorityId);

                if (priorityRow != null)
                {
                    priorityRow.Name = PriorityName;
                    _prioritiesTableAdapter.Update(_dataSet.Priorities);

                    try
                    {
                        _userActivityLogger.LogUserActivity(GetUserId(_currentUserLogin), _currentUserLogin, $"успешно обновил приоритет с ID {PriorityId} и именем {PriorityName}");
                        MessageBox.Show("Приоритет успешно обновлен!");
                    }
                    catch (Exception logEx)
                    {
                        MessageBox.Show($"Ошибка при логировании: {logEx.Message}");
                    }

                    PriorityUpdated?.Invoke(this, EventArgs.Empty);
                    Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this)?.Close();
                    return;
                }
                else
                {
                    MessageBox.Show("Приоритет не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {

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
