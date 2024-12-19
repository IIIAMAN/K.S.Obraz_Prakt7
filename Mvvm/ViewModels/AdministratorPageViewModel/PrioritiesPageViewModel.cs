using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TaskManagement.Helpers;
using TaskManagement.Models;
using TaskManagement.MVVM.Views.AdministratorPages.PrioritiesPage;
using TaskManagement.TaskManagementDataSetTableAdapters;
using TaskManagement.ViewModels;

namespace TaskManagement.MVVM.ViewModels.AdministratorPageViewModel
{
    public class PrioritiesPageViewModel : ViewModelBase
    {
        private TaskManagementDataSet _dataSet;
        private PrioritiesTableAdapter _prioritiesTableAdapter;
        private readonly UserActivityLogger _userActivityLogger;

        private ObservableCollection<dynamic> _priorities;
        private dynamic _selectedPriority;
        private string _currentUserLogin;

        public PrioritiesPageViewModel()
        {
            _dataSet = new TaskManagementDataSet();
            _prioritiesTableAdapter = new PrioritiesTableAdapter();
            _userActivityLogger = new UserActivityLogger(ConfigurationManager.ConnectionStrings["TaskManagement.Properties.Settings.TaskManagementConnectionString"].ConnectionString);

            LoadPriorities();

            AddPriorityCommand = new RelayCommand(AddPriority);
            EditPriorityCommand = new RelayCommand(EditPriority, CanEditOrDeletePriority);
            DeletePriorityCommand = new RelayCommand(DeletePriority, CanEditOrDeletePriority);
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

        public ObservableCollection<dynamic> Priorities
        {
            get => _priorities;
            set
            {
                _priorities = value;
                OnPropertyChanged();
            }
        }

        public dynamic SelectedPriority
        {
            get => _selectedPriority;
            set
            {
                _selectedPriority = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddPriorityCommand { get; }
        public ICommand EditPriorityCommand { get; }
        public ICommand DeletePriorityCommand { get; }

        private void LoadPriorities()
        {
            _prioritiesTableAdapter.Fill(_dataSet.Priorities);
            Priorities = new ObservableCollection<dynamic>(_dataSet.Priorities.Cast<dynamic>().ToList());
        }

        private void AddPriority(object parameter)
        {
            var addPriorityWindow = new PriorityAddWindow(CurrentUserLogin);
            var viewModel = (PriorityAddViewModel)addPriorityWindow.DataContext;

            viewModel.PriorityAdded += (s, args) =>
            {
                LoadPriorities();
                _userActivityLogger.LogUserActivity(GetUserId(CurrentUserLogin), CurrentUserLogin, $"добавил новый приоритет: {viewModel.PriorityName}");
            };

            if (addPriorityWindow.ShowDialog() == true)
            {
                LoadPriorities();
            }
        }

        private void EditPriority(object parameter)
        {
            if (SelectedPriority != null)
            {
                var editWindow = new PriorityEditWindow(SelectedPriority.ID, CurrentUserLogin);
                var viewModel = (PriorityEditViewModel)editWindow.DataContext;

                viewModel.PriorityUpdated += (s, args) =>
                {
                    LoadPriorities();
                    _userActivityLogger.LogUserActivity(GetUserId(CurrentUserLogin), CurrentUserLogin, $"обновил приоритет: {SelectedPriority.Name}");
                };

                if (editWindow.ShowDialog() == true)
                {
                    LoadPriorities();
                }
            }
        }

        private void DeletePriority(object parameter)
        {
            if (SelectedPriority != null)
            {
                if (MessageBox.Show("Вы уверены, что хотите удалить этот приоритет?", "Удаление приоритета", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        var priorityRow = _dataSet.Priorities.FindByID(SelectedPriority.ID);
                        if (priorityRow != null)
                        {
                            string priorityName = priorityRow.Name;
                            priorityRow.Delete();
                            _prioritiesTableAdapter.Update(_dataSet.Priorities);

                            _userActivityLogger.LogUserActivity(GetUserId(CurrentUserLogin), CurrentUserLogin, $"удалил приоритет: {priorityName}");

                            LoadPriorities();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении приоритета: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private bool CanEditOrDeletePriority(object parameter)
        {
            return SelectedPriority != null;
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
