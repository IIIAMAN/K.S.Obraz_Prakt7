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
    public class MetadataEditViewModel : ViewModelBase
    {
        private TaskManagementDataSet _dataSet;
        private MetadataTableAdapter _metadataTableAdapter;
        private readonly UserActivityLogger _userActivityLogger;
        private string _currentUserLogin;

        private int _metadataId;
        private string _entityName;
        private int _entityID;
        private string _key;
        private string _value;

        public int MetadataId
        {
            get => _metadataId;
            private set
            {
                _metadataId = value;
                OnPropertyChanged();
            }
        }

        public string EntityName
        {
            get => _entityName;
            set
            {
                _entityName = value;
                OnPropertyChanged();
            }
        }

        public int EntityID
        {
            get => _entityID;
            set
            {
                _entityID = value;
                OnPropertyChanged();
            }
        }

        public string Key
        {
            get => _key;
            set
            {
                _key = value;
                OnPropertyChanged();
            }
        }

        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler MetadataUpdated;

        public MetadataEditViewModel(int metadataId, string currentUserLogin)
        {
            _dataSet = new TaskManagementDataSet();
            _metadataTableAdapter = new MetadataTableAdapter();
            _userActivityLogger = new UserActivityLogger(ConfigurationManager.ConnectionStrings["TaskManagement.Properties.Settings.TaskManagementConnectionString"].ConnectionString);
            _currentUserLogin = currentUserLogin;

            MetadataId = metadataId;

            LoadMetadataData(metadataId);

            SaveCommand = new RelayCommand(SaveMetadata);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void LoadMetadataData(int metadataId)
        {
            _metadataTableAdapter.Fill(_dataSet.Metadata);
            var metadataRow = _dataSet.Metadata.FindByID(metadataId);

            if (metadataRow != null)
            {
                EntityName = metadataRow.EntityName;
                EntityID = metadataRow.EntityID;
                Key = metadataRow.Key;
                Value = metadataRow.Value;
            }
        }

        private void SaveMetadata(object parameter)
        {
            try
            {
                if (string.IsNullOrEmpty(EntityName) || string.IsNullOrEmpty(Key) || string.IsNullOrEmpty(Value))
                {
                    MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var metadataRow = _dataSet.Metadata.FindByID(MetadataId);

                if (metadataRow != null)
                {
                    metadataRow.EntityName = EntityName;
                    metadataRow.EntityID = EntityID;
                    metadataRow.Key = Key;
                    metadataRow.Value = Value;

                    _metadataTableAdapter.Update(metadataRow);

                    // Логирование успешного обновления метаданных
                    _userActivityLogger.LogUserActivity(GetUserId(_currentUserLogin), _currentUserLogin, $"успешно обновил метаданные для сущности {EntityName} с ключом {Key}");

                    MessageBox.Show("Метаданные успешно обновлены!");

                    MetadataUpdated?.Invoke(this, EventArgs.Empty);

                    Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this)?.Close();
                }
                else
                {
                    MessageBox.Show("Метаданные не найдены.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении метаданных: {ex.Message}");
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
