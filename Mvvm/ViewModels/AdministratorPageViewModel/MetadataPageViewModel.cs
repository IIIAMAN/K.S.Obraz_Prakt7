using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TaskManagement.Helpers;
using TaskManagement.Models;
using TaskManagement.MVVM.Views.AdministratorPages.MetadataPage;
using TaskManagement.TaskManagementDataSetTableAdapters;
using TaskManagement.ViewModels;
using System.Configuration;

namespace TaskManagement.MVVM.ViewModels.AdministratorPageViewModel
{
    public class MetadataPageViewModel : ViewModelBase
    {
        private TaskManagementDataSet _dataSet;
        private MetadataTableAdapter _metadataTableAdapter;
        private readonly UserActivityLogger _userActivityLogger;
        private string _currentUserLogin;
        private ObservableCollection<TaskManagementDataSet.MetadataRow> _metadata;
        private TaskManagementDataSet.MetadataRow _selectedMetadata;

        public ObservableCollection<TaskManagementDataSet.MetadataRow> Metadata
        {
            get => _metadata;
            set
            {
                _metadata = value;
                OnPropertyChanged();
            }
        }

        public TaskManagementDataSet.MetadataRow SelectedMetadata
        {
            get => _selectedMetadata;
            set
            {
                _selectedMetadata = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddMetadataCommand { get; }
        public ICommand EditMetadataCommand { get; }
        public ICommand DeleteMetadataCommand { get; }

        public MetadataPageViewModel(string currentUserLogin)
        {
            _dataSet = new TaskManagementDataSet();
            _metadataTableAdapter = new MetadataTableAdapter();
            _userActivityLogger = new UserActivityLogger(ConfigurationManager.ConnectionStrings["TaskManagement.Properties.Settings.TaskManagementConnectionString"].ConnectionString);
            _currentUserLogin = currentUserLogin;

            LoadMetadata();

            AddMetadataCommand = new RelayCommand(AddMetadata);
            EditMetadataCommand = new RelayCommand(EditMetadata, CanEditOrDeleteMetadata);
            DeleteMetadataCommand = new RelayCommand(DeleteMetadata, CanEditOrDeleteMetadata);
        }

        public void LoadMetadata()
        {
            _metadataTableAdapter.Fill(_dataSet.Metadata);
            Metadata = new ObservableCollection<TaskManagementDataSet.MetadataRow>(_dataSet.Metadata.Cast<TaskManagementDataSet.MetadataRow>());
        }

        private void AddMetadata(object parameter)
        {
            var addMetadataWindow = new MetadataAddWindow(_currentUserLogin);
            var viewModel = (MetadataAddViewModel)addMetadataWindow.DataContext;

            viewModel.MetadataAdded += (s, args) =>
            {
                LoadMetadata();
            };

            addMetadataWindow.ShowDialog();
        }

        private void EditMetadata(object parameter)
        {
            if (SelectedMetadata != null)
            {
                var editWindow = new MetadataEditWindow(SelectedMetadata.ID, _currentUserLogin);
                var viewModel = (MetadataEditViewModel)editWindow.DataContext;

                viewModel.MetadataUpdated += (s, args) =>
                {
                    LoadMetadata();
                };

                if (editWindow.ShowDialog() == true)
                {
                    LoadMetadata();
                }
            }
        }

        private void DeleteMetadata(object parameter)
        {
            if (SelectedMetadata != null)
            {
                if (MessageBox.Show("Вы уверены, что хотите удалить эту запись?", "Удаление записи", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        var metadataRow = _dataSet.Metadata.FindByID(SelectedMetadata.ID);
                        if (metadataRow != null)
                        {
                            metadataRow.Delete();
                            _metadataTableAdapter.Update(_dataSet.Metadata);

                            _userActivityLogger.LogUserActivity(GetUserId(_currentUserLogin), _currentUserLogin, $"успешно удалил метаданные");

                            LoadMetadata();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении записи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private bool CanEditOrDeleteMetadata(object parameter)
        {
            return SelectedMetadata != null;
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
