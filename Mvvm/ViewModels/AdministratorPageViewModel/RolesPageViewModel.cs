using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TaskManagement.Helpers;
using TaskManagement.MVVM.Views.AdministratorPages.RolePage;
using TaskManagement.TaskManagementDataSetTableAdapters;
using TaskManagement.ViewModels;
using System.Configuration;

namespace TaskManagement.MVVM.ViewModels.AdministratorPageViewModel
{
    public class RolesPageViewModel : ViewModelBase
    {
        private TaskManagementDataSet _dataSet;
        private RolesTableAdapter _rolesTableAdapter;
        private readonly UserActivityLogger _userActivityLogger;
        private ObservableCollection<TaskManagementDataSet.RolesRow> _roles;
        private TaskManagementDataSet.RolesRow _selectedRole;
        private string _currentUserLogin;

        public ObservableCollection<TaskManagementDataSet.RolesRow> Roles
        {
            get => _roles;
            set
            {
                _roles = value;
                OnPropertyChanged();
            }
        }

        public TaskManagementDataSet.RolesRow SelectedRole
        {
            get => _selectedRole;
            set
            {
                _selectedRole = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddRoleCommand { get; }
        public ICommand EditRoleCommand { get; }
        public ICommand DeleteRoleCommand { get; }

        public string CurrentUserLogin
        {
            get => _currentUserLogin;
            set
            {
                _currentUserLogin = value;
                OnPropertyChanged();
            }
        }

        public RolesPageViewModel()
        {
            _dataSet = new TaskManagementDataSet();
            _rolesTableAdapter = new RolesTableAdapter();
            _userActivityLogger = new UserActivityLogger(ConfigurationManager.ConnectionStrings["TaskManagement.Properties.Settings.TaskManagementConnectionString"].ConnectionString);

            LoadRoles();

            AddRoleCommand = new RelayCommand(AddRole);
            EditRoleCommand = new RelayCommand(EditRole, CanEditOrDeleteRole);
            DeleteRoleCommand = new RelayCommand(DeleteRole, CanEditOrDeleteRole);
        }

        public void LoadRoles()
        {
            _rolesTableAdapter.Fill(_dataSet.Roles);
            Roles = new ObservableCollection<TaskManagementDataSet.RolesRow>(_dataSet.Roles.Cast<TaskManagementDataSet.RolesRow>());
        }

        private void AddRole(object parameter)
        {
            var addRoleWindow = new RoleAddWindow(CurrentUserLogin); // Передаем логин текущего пользователя
            var viewModel = (RoleAddViewModel)addRoleWindow.DataContext;

            viewModel.RoleAdded += (s, args) =>
            {
                LoadRoles();
                _userActivityLogger.LogUserActivity(GetUserId(CurrentUserLogin), CurrentUserLogin, $"добавил новую роль: {viewModel.RoleName}");
            };

            addRoleWindow.ShowDialog();
        }

        private void EditRole(object parameter)
        {
            if (SelectedRole != null)
            {
                var editWindow = new RoleEditWindow(SelectedRole.ID, CurrentUserLogin); // Передаем логин текущего пользователя
                var viewModel = (RoleEditViewModel)editWindow.DataContext;

                viewModel.RoleUpdated += (s, args) =>
                {
                    LoadRoles();
                    _userActivityLogger.LogUserActivity(GetUserId(CurrentUserLogin), CurrentUserLogin, $"Обновил роль");
                };

                if (editWindow.ShowDialog() == true)
                {
                    LoadRoles();
                }
            }
        }

        private void DeleteRole(object parameter)
        {
            if (SelectedRole != null)
            {
                var essentialRoles = new string[] { "TaskManager", "ChiefTaskExecutor", "TaskExecutor", "Administrator" };
                if (essentialRoles.Contains(SelectedRole.Name))
                {
                    MessageBox.Show("Невозможно удалить основную роль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (MessageBox.Show("Вы уверены, что хотите удалить эту роль?", "Удаление роли", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        var roleRow = _dataSet.Roles.FindByID(SelectedRole.ID);
                        if (roleRow != null)
                        {
                            roleRow.Delete();
                            _rolesTableAdapter.Update(_dataSet.Roles);

                            _userActivityLogger.LogUserActivity(GetUserId(CurrentUserLogin), CurrentUserLogin, $"удалил роль");

                            LoadRoles();
                        }
                        else
                        {
                            MessageBox.Show("Роль не найдена.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении роли: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private bool CanEditOrDeleteRole(object parameter)
        {
            return SelectedRole != null;
        }

        private int GetUserId(string login)
        {
            var userRow = GetUserFromDatabase(login);
            return userRow?.ID ?? 0; // Если пользователь не найден, возвращаем 0
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
