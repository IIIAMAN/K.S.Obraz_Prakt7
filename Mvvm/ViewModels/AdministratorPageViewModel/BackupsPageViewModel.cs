using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TaskManagement.Models;
using TaskManagement.MVVM.Views.AdministratorPages.BackupsPage;
using TaskManagement.TaskManagementDataSetTableAdapters;
using TaskManagement.ViewModels;
using System.Configuration;
using TaskManagement.Helpers;

namespace TaskManagement.MVVM.ViewModels.AdministratorPageViewModel
{
    public class BackupsPageViewModel : ViewModelBase
    {
        private const string BackupDirectory = "D:\\ProjectsTestirovanie\\TaskManagement\\Backups\\";
        private TaskManagementDataSet _dataSet;
        private BackupsTableAdapter _backupsTableAdapter;
        private readonly UserActivityLogger _userActivityLogger;
        private string _currentUserLogin;
        private ObservableCollection<TaskManagementDataSet.BackupsRow> _backups;
        private TaskManagementDataSet.BackupsRow _selectedBackup;

        public ObservableCollection<TaskManagementDataSet.BackupsRow> Backups
        {
            get => _backups;
            set
            {
                _backups = value;
                OnPropertyChanged();
            }
        }

        public TaskManagementDataSet.BackupsRow SelectedBackup
        {
            get => _selectedBackup;
            set
            {
                _selectedBackup = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddBackupCommand { get; }
        public ICommand EditBackupCommand { get; }
        public ICommand DeleteBackupCommand { get; }
        public ICommand CreateBackupCommand { get; }
        public ICommand RestoreBackupCommand { get; }

        public BackupsPageViewModel(string currentUserLogin)
        {
            _currentUserLogin = currentUserLogin;
            _dataSet = new TaskManagementDataSet();
            _backupsTableAdapter = new BackupsTableAdapter();
            _userActivityLogger = new UserActivityLogger(ConfigurationManager.ConnectionStrings["TaskManagement.Properties.Settings.TaskManagementConnectionString"].ConnectionString);

            LoadBackups();

            AddBackupCommand = new RelayCommand(AddBackup);
            EditBackupCommand = new RelayCommand(EditBackup, CanEditOrDeleteBackup);
            DeleteBackupCommand = new RelayCommand(DeleteBackup, CanEditOrDeleteBackup);
            CreateBackupCommand = new RelayCommand(CreateBackup);
            RestoreBackupCommand = new RelayCommand(RestoreBackup, CanEditOrDeleteBackup);
        }

        public void LoadBackups()
        {
            _backupsTableAdapter.Fill(_dataSet.Backups);
            Backups = new ObservableCollection<TaskManagementDataSet.BackupsRow>(_dataSet.Backups.Cast<TaskManagementDataSet.BackupsRow>());
        }

        private void AddBackup(object parameter)
        {
            var addBackupWindow = new BackupsAddWindow(_currentUserLogin);
            var viewModel = (BackupsAddViewModel)addBackupWindow.DataContext;

            viewModel.BackupAdded += (s, args) => LoadBackups();

            addBackupWindow.ShowDialog();
        }

        private void EditBackup(object parameter)
        {
            if (SelectedBackup != null)
            {
                var editWindow = new BackupsEditWindow(SelectedBackup.ID, _currentUserLogin);
                var viewModel = (BackupsEditViewModel)editWindow.DataContext;

                viewModel.BackupUpdated += (s, args) => LoadBackups();

                if (editWindow.ShowDialog() == true)
                {
                    LoadBackups();
                }
            }
        }

        private void DeleteBackup(object parameter)
        {
            if (SelectedBackup != null &&
                MessageBox.Show("Вы уверены, что хотите удалить эту резервную копию?", "Удаление резервной копии", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    var backupRow = _dataSet.Backups.FindByID(SelectedBackup.ID);

                    string backupFilePath = SelectedBackup.BackupFilePath;
                    if (File.Exists(backupFilePath))
                    {
                        File.Delete(backupFilePath);
                    }

                    if (backupRow != null)
                    {
                        backupRow.Delete();
                        _backupsTableAdapter.Update(_dataSet.Backups);
                        LoadBackups();
                    }

                    _userActivityLogger.LogUserActivity(GetUserId(_currentUserLogin), _currentUserLogin, $"Удалил резервную копию");
                    MessageBox.Show("Резервная копия успешно удалена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении резервной копии: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CreateBackup(object parameter)
        {
            try
            {
                string fileName = $"Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                string filePath = Path.Combine(BackupDirectory, fileName);

                if (!Directory.Exists(BackupDirectory))
                    Directory.CreateDirectory(BackupDirectory);

                _backupsTableAdapter.Insert(DateTime.Now, filePath);
                LoadBackups();

                string connectionString = ConfigurationManager.ConnectionStrings["TaskManagement.Properties.Settings.TaskManagementConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string backupCommand = $@"
                        BACKUP DATABASE [TaskManagement] 
                        TO DISK = @BackupPath";

                    using (var command = new SqlCommand(backupCommand, connection))
                    {
                        command.Parameters.AddWithValue("@BackupPath", filePath);
                        command.ExecuteNonQuery();
                    }
                }

                AddMissingBackups();
                _userActivityLogger.LogUserActivity(GetUserId(_currentUserLogin), _currentUserLogin, $"Создал резервную копию с путём {filePath}");
                MessageBox.Show("Резервная копия успешно создана.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании резервной копии: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RestoreBackup(object parameter)
        {
            if (SelectedBackup != null)
            {
                try
                {
                    string backupPath = SelectedBackup.BackupFilePath;

                    if (File.Exists(backupPath))
                    {
                        string connectionString = ConfigurationManager.ConnectionStrings["TaskManagement.Properties.Settings.TaskManagementConnectionString"].ConnectionString;

                        using (var connection = new SqlConnection(connectionString))
                        {
                            connection.Open();

                            using (var command = new SqlCommand("USE master", connection))
                            {
                                command.ExecuteNonQuery();
                            }

                            string disconnectCommand = "ALTER DATABASE [TaskManagement] SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
                            using (var command = new SqlCommand(disconnectCommand, connection))
                            {
                                command.ExecuteNonQuery();
                            }

                            string restoreCommandText = $@"
                                RESTORE DATABASE [TaskManagement] 
                                FROM DISK = @BackupPath 
                                WITH REPLACE";

                            using (var command = new SqlCommand(restoreCommandText, connection))
                            {
                                command.Parameters.AddWithValue("@BackupPath", backupPath);
                                command.ExecuteNonQuery();
                            }

                            string multiUserCommand = "ALTER DATABASE [TaskManagement] SET MULTI_USER";
                            using (var command = new SqlCommand(multiUserCommand, connection))
                            {
                                command.ExecuteNonQuery();
                            }
                        }

                        LoadAllBackups();
                        _userActivityLogger.LogUserActivity(GetUserId(_currentUserLogin), _currentUserLogin, $"Восстановил резервную копию с путём {backupPath}");
                        MessageBox.Show("Резервная копия успешно восстановлена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Файл резервной копии не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при восстановлении резервной копии: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LoadAllBackups()
        {
            try
            {
                _backupsTableAdapter.Fill(_dataSet.Backups);

                var backupFiles = Directory.GetFiles(BackupDirectory, "*.bak");
                foreach (var backupFile in backupFiles)
                {
                    var existingBackup = _dataSet.Backups
                                                 .Cast<TaskManagementDataSet.BackupsRow>()
                                                 .FirstOrDefault(b => b.BackupFilePath == backupFile);

                    if (existingBackup == null)
                    {
                        _backupsTableAdapter.Insert(DateTime.Now, backupFile);
                    }
                }

                LoadBackups();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке резервных копий: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddMissingBackups()
        {
            try
            {
                var backupFiles = Directory.GetFiles(BackupDirectory, "*.bak");

                foreach (var filePath in backupFiles)
                {
                    if (!_dataSet.Backups.Any(b => b.BackupFilePath == filePath))
                    {
                        _backupsTableAdapter.Insert(DateTime.Now, filePath);
                    }
                }

                LoadBackups();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении резервных копий в таблицу: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanEditOrDeleteBackup(object parameter) => SelectedBackup != null;

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
