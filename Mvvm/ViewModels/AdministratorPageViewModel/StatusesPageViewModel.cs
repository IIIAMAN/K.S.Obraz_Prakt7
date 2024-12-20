using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TaskManagement.Helpers;
using TaskManagement.Models;
using TaskManagement.MVVM.Views.AdministratorPages.StatusPage;
using TaskManagement.TaskManagementDataSetTableAdapters;
using TaskManagement.ViewModels;
using System.Configuration;

namespace TaskManagement.MVVM.ViewModels.AdministratorPageViewModel
{
    public class StatusesPageViewModel : ViewModelBase
    {
        private TaskManagementDataSet _dataSet;
        private StatusesTableAdapter _statusesTableAdapter;
        private readonly UserActivityLogger _userActivityLogger;
        private ObservableCollection<TaskManagementDataSet.StatusesRow> _statuses;
        private TaskManagementDataSet.StatusesRow _selectedStatus;
        private string _currentUserLogin;

        public ObservableCollection<TaskManagementDataSet.StatusesRow> Statuses
        {
            get => _statuses;
            set
            {
                _statuses = value;
                OnPropertyChanged();
            }
        }

        public TaskManagementDataSet.StatusesRow SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                _selectedStatus = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddStatusCommand { get; }
        public ICommand EditStatusCommand { get; }
        public ICommand DeleteStatusCommand { get; }

        public string CurrentUserLogin
        {
            get => _currentUserLogin;
            set
            {
                _currentUserLogin = value;
                OnPropertyChanged();
            }
        }

        public StatusesPageViewModel()
        {
            _dataSet = new TaskManagementDataSet();
            _statusesTableAdapter = new StatusesTableAdapter();
            _userActivityLogger = new UserActivityLogger(ConfigurationManager.ConnectionStrings["TaskManagement.Properties.Settings.TaskManagementConnectionString"].ConnectionString);

            LoadStatuses();

            AddStatusCommand = new RelayCommand(AddStatus);
            EditStatusCommand = new RelayCommand(EditStatus, CanEditOrDeleteStatus);
            DeleteStatusCommand = new RelayCommand(DeleteStatus, CanEditOrDeleteStatus);
        }

        public void LoadStatuses()
        {
            _statusesTableAdapter.Fill(_dataSet.Statuses);
            Statuses = new ObservableCollection<TaskManagementDataSet.StatusesRow>(_dataSet.Statuses.Cast<TaskManagementDataSet.StatusesRow>());
        }

        private void AddStatus(object parameter)
        {
            var addStatusWindow = new StatusAddWindow(CurrentUserLogin);
            var viewModel = (StatusAddViewModel)addStatusWindow.DataContext;

            viewModel.StatusAdded += (s, args) =>
            {
                LoadStatuses();
                _userActivityLogger.LogUserActivity(GetUserId(CurrentUserLogin), CurrentUserLogin, $"добавил новый статус: {viewModel.StatusName}");
            };

            addStatusWindow.ShowDialog();
        }

        private void EditStatus(object parameter)
        {
            if (SelectedStatus != null)
            {
                var editWindow = new StatusEditWindow(SelectedStatus.ID, CurrentUserLogin); 
                var viewModel = (StatusEditViewModel)editWindow.DataContext;

                viewModel.StatusUpdated += (s, args) =>
                {
                    LoadStatuses();
                    _userActivityLogger.LogUserActivity(GetUserId(CurrentUserLogin), CurrentUserLogin, $"обновил статус с ID: {SelectedStatus.ID} на {viewModel.StatusName}");
                };

                if (editWindow.ShowDialog() == true)
                {
                    LoadStatuses();
                }
            }
        }

        private void DeleteStatus(object parameter)
        {
            if (SelectedStatus != null)
            {
                if (MessageBox.Show("Вы уверены, что хотите удалить этот статус?", "Удаление статуса", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        var statusRow = _dataSet.Statuses.FindByID(SelectedStatus.ID);
                        if (statusRow != null)
                        {
                            statusRow.Delete();
                            _statusesTableAdapter.Update(_dataSet.Statuses);
                            LoadStatuses(); 


                            _userActivityLogger.LogUserActivity(GetUserId(CurrentUserLogin), CurrentUserLogin, $"удалил статус: {SelectedStatus.Name}");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении статуса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private bool CanEditOrDeleteStatus(object parameter)
        {
            return SelectedStatus != null;
        }

        private int GetUserId(string login)
        {
            var userRow = _dataSet.Users.FirstOrDefault(u => u.Login == login);
            return userRow?.ID ?? 0;
        }
    }
}
