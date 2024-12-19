using System;
using System.Configuration;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TaskManagement.Helpers;
using TaskManagement.TaskManagementDataSetTableAdapters;
using TaskManagement.ViewModels;

namespace TaskManagement.MVVM.ViewModels.AdministratorPageViewModel
{
    public class RoleEditViewModel : ViewModelBase
    {
        private TaskManagementDataSet _dataSet;
        private RolesTableAdapter _rolesTableAdapter;
        private readonly UserActivityLogger _userActivityLogger;
        private int _roleId;
        private string _roleName;
        private string _currentUserLogin;

        public int RoleId
        {
            get => _roleId;
            private set
            {
                _roleId = value;
                OnPropertyChanged();
            }
        }

        public string RoleName
        {
            get => _roleName;
            set
            {
                _roleName = value;
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

        public event EventHandler RoleUpdated;

        public RoleEditViewModel(int roleId, string currentUserLogin)
        {
            _dataSet = new TaskManagementDataSet();
            _rolesTableAdapter = new RolesTableAdapter();
            _userActivityLogger = new UserActivityLogger(ConfigurationManager.ConnectionStrings["TaskManagement.Properties.Settings.TaskManagementConnectionString"].ConnectionString);
            _currentUserLogin = currentUserLogin;

            RoleId = roleId;

            LoadRole(roleId);

            SaveCommand = new RelayCommand(SaveRole);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void LoadRole(int roleId)
        {
            _rolesTableAdapter.Fill(_dataSet.Roles);
            var roleRow = _dataSet.Roles.FindByID(roleId);

            if (roleRow != null)
            {
                RoleName = roleRow.Name;
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

                var existingRole = _dataSet.Roles.FirstOrDefault(r => r.Name == RoleName && r.ID != RoleId);
                if (existingRole != null)
                {
                    MessageBox.Show("Роль с таким именем уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var roleRow = _dataSet.Roles.FindByID(RoleId);

                if (roleRow != null)
                {
                    string oldRoleName = roleRow.Name;

                    roleRow.Name = RoleName;
                    _rolesTableAdapter.Update(_dataSet.Roles);

                    _userActivityLogger.LogUserActivity(GetUserId(CurrentUserLogin), CurrentUserLogin, $"обновил роль с именем: {oldRoleName} на {RoleName}");

                    RoleUpdated?.Invoke(this, EventArgs.Empty);

                    Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this)?.Close();
                }
                else
                {
                    MessageBox.Show("Роль не найдена.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении роли: {ex.Message}");
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
