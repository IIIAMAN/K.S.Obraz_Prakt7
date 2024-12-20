using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TaskManagement.Helpers;
using TaskManagement.Models;
using TaskManagement.MVVM.Views.AdministratorPages.UserPage;
using TaskManagement.TaskManagementDataSetTableAdapters;

namespace TaskManagement.ViewModels.AdministratorPageViewModels
{
    public class UsersPageViewModel : ViewModelBase
    {
        private TaskManagementDataSet _dataSet;
        private UsersTableAdapter _usersTableAdapter;
        private AccountDataTableAdapter _accountDataTableAdapter;
        private RolesTableAdapter _rolesTableAdapter;
        private readonly UserActivityLogger _userActivityLogger;

        private ObservableCollection<dynamic> _users;
        private dynamic _selectedUser;
        private string _currentUserLogin;

        public UsersPageViewModel()
        {
            _dataSet = new TaskManagementDataSet();
            _usersTableAdapter = new UsersTableAdapter();
            _accountDataTableAdapter = new AccountDataTableAdapter();
            _rolesTableAdapter = new RolesTableAdapter();
            _userActivityLogger = new UserActivityLogger(ConfigurationManager.ConnectionStrings["TaskManagement.Properties.Settings.TaskManagementConnectionString"].ConnectionString);

            LoadUsers();

            AddUserCommand = new RelayCommand(AddUser);
            EditUserCommand = new RelayCommand(EditUser, CanEditOrDeleteUser);
            DeleteUserCommand = new RelayCommand(DeleteUser, CanEditOrDeleteUser);
            SelectUserCommand = new RelayCommand(SelectUser);
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

        public ObservableCollection<dynamic> Users
        {
            get => _users;
            set
            {
                _users = value;
                OnPropertyChanged();
            }
        }

        public dynamic SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddUserCommand { get; }
        public ICommand EditUserCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand SelectUserCommand { get; }

        private void LoadUsers()
        {
            _usersTableAdapter.Fill(_dataSet.Users);
            _accountDataTableAdapter.Fill(_dataSet.AccountData);
            _rolesTableAdapter.Fill(_dataSet.Roles);

            var userList = from user in _dataSet.Users
                           join account in _dataSet.AccountData on user.ID equals account.UserID into accountData
                           from account in accountData.DefaultIfEmpty()
                           join role in _dataSet.Roles on user.RoleID equals role.ID
                           select new
                           {
                               user.ID,
                               user.Login,
                               Role = role.Name,
                               Password = user.Password,
                               user.CreatedAt,
                               UpdatedAt = user.IsUpdatedAtNull() ? "null" : user.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                               account?.FirstName,
                               account?.LastName,
                               account?.PhoneNumber,
                               account?.ProfileDescription
                           };

            Users = new ObservableCollection<dynamic>(userList.ToList());
        }

        private void AddUser(object parameter)
        {
            var addUserWindow = new UserAddWindow(CurrentUserLogin);
            if (addUserWindow.ShowDialog() == true)
            {
                LoadUsers();
            }
        }

        private void EditUser(object parameter)
        {
            if (SelectedUser != null)
            {
                var userRow = _dataSet.Users.FindByID(SelectedUser.ID);
                var editWindow = new UserEditWindow(userRow, CurrentUserLogin);
                if (editWindow.ShowDialog() == true)
                {
                    LoadUsers();
                }
            }
        }

        private void DeleteUser(object parameter)
        {
            if (SelectedUser != null)
            {
                Console.WriteLine($"Текущий пользователь: {_currentUserLogin}");
                Console.WriteLine($"Пользователь для удаления: {SelectedUser.Login}");

                if (SelectedUser.Login == _currentUserLogin)
                {
                    MessageBox.Show("Вы не можете удалить текущего пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var userRow = _dataSet.Users.FindByID(SelectedUser.ID);
                if (userRow != null)
                {
                    if (MessageBox.Show("Вы уверены, что хотите удалить этого пользователя?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        try
                        {
                            var accountRow = _dataSet.AccountData.FirstOrDefault(a => a.UserID == userRow.ID);
                            if (accountRow != null)
                            {
                                accountRow.Delete();
                            }

                            _accountDataTableAdapter.Update(_dataSet.AccountData);

                            userRow.Delete();
                            _usersTableAdapter.Update(_dataSet.Users);

                            _userActivityLogger.LogUserActivity(GetUserId(_currentUserLogin), _currentUserLogin, $"успешно удалил пользователя {SelectedUser.Login} из таблицы Users");

                            Console.WriteLine("Пользователь успешно удален.");
                            LoadUsers();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка при удалении пользователя: {ex.Message}");
                            MessageBox.Show($"Ошибка при удалении пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        private void SelectUser(object parameter)
        {
            SelectedUser = parameter;
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

        private bool CanEditOrDeleteUser(object parameter)
        {
            return SelectedUser != null;
        }
    }
}
