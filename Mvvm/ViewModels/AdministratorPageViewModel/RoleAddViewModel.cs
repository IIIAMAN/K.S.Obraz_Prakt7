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
    public class RoleAddViewModel : ViewModelBase
    {
        private TaskManagementDataSet _dataSet;
        private RolesTableAdapter _rolesTableAdapter;
        private readonly UserActivityLogger _userActivityLogger;
        private string _roleName;

        public string RoleName
        {
            get => _roleName;
            set
            {
                _roleName = value;
                OnPropertyChanged();
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler RoleAdded;

        public RoleAddViewModel(string currentUserLogin)
        {
            _dataSet = new TaskManagementDataSet();
            _rolesTableAdapter = new RolesTableAdapter();
            _userActivityLogger = new UserActivityLogger(ConfigurationManager.ConnectionStrings["TaskManagement.Properties.Settings.TaskManagementConnectionString"].ConnectionString);

            SaveCommand = new RelayCommand(SaveRole);
            CancelCommand = new RelayCommand(Cancel);

            CurrentUserLogin = currentUserLogin;
        }

        private string _currentUserLogin;

        public string CurrentUserLogin
        {
            get => _currentUserLogin;
            set
            {
                _currentUserLogin = value;
                OnPropertyChanged();
            }
        }

        private void SaveRole(object parameter)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(RoleName))
                {
                    MessageBox.Show("Имя роли не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var newRoleRow = _dataSet.Roles.NewRolesRow();
                newRoleRow.Name = RoleName;

                _dataSet.Roles.AddRolesRow(newRoleRow);
                _rolesTableAdapter.Update(_dataSet.Roles);

                MessageBox.Show("Роль успешно добавлена!");

                // Логирование действия пользователя
                _userActivityLogger.LogUserActivity(GetUserId(CurrentUserLogin), CurrentUserLogin, $"добавил новую роль: {RoleName}");

                RoleAdded?.Invoke(this, EventArgs.Empty);

                Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this)?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении роли: {ex.Message}");
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
