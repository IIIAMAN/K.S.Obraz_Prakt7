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
    public class MetadataAddViewModel : ViewModelBase
    {
        private TaskManagementDataSet _dataSet;
        private MetadataTableAdapter _metadataTableAdapter;
        private readonly UserActivityLogger _userActivityLogger;
        private string _currentUserLogin;

        private string _entityName;
        private int _entityID;
        private string _key;
        private string _value;

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

        public event EventHandler MetadataAdded;

        public MetadataAddViewModel(string currentUserLogin)
        {
            _dataSet = new TaskManagementDataSet();
            _metadataTableAdapter = new MetadataTableAdapter();
            _userActivityLogger = new UserActivityLogger(ConfigurationManager.ConnectionStrings["TaskManagement.Properties.Settings.TaskManagementConnectionString"].ConnectionString);
            _currentUserLogin = currentUserLogin;

            SaveCommand = new RelayCommand(SaveMetadata);
            CancelCommand = new RelayCommand(Cancel);
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

                _metadataTableAdapter.Insert(EntityName, EntityID, Key, Value);

                _userActivityLogger.LogUserActivity(GetUserId(_currentUserLogin), _currentUserLogin, $"успешно добавил метаданные для сущности {EntityName} с ключом {Key}");

                MessageBox.Show("Метаданные успешно добавлены!");

                MetadataAdded?.Invoke(this, EventArgs.Empty);

                Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this)?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении метаданных: {ex.Message}");
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
