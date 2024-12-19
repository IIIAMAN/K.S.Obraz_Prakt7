using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;
using TaskManagement.Helpers;
using TaskManagement.Models;
using TaskManagement.TaskManagementDataSetTableAdapters;
using System.Configuration;

namespace TaskManagement.ViewModels.AdministratorPageViewModels
{
    public class UserAddViewModel : ViewModelBase
    {
        private TaskManagementDataSet _dataSet;
        private UsersTableAdapter _usersTableAdapter;
        private AccountDataTableAdapter _accountDataTableAdapter;
        private RolesTableAdapter _rolesTableAdapter;
        private UserActivityLogger _userActivityLogger;

        private string _login;
        private SecureString _password;
        private string _firstName;
        private string _lastName;
        private string _phoneNumber;
        private string _profileDescription;
        private int _selectedRoleId;
        private string _currentUserLogin;

        public string Login
        {
            get => _login;
            set
            {
                _login = value;
                OnPropertyChanged();
            }
        }

        public SecureString Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        public string FirstName
        {
            get => _firstName;
            set
            {
                _firstName = value;
                OnPropertyChanged();
            }
        }

        public string LastName
        {
            get => _lastName;
            set
            {
                _lastName = value;
                OnPropertyChanged();
            }
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                _phoneNumber = value;
                OnPropertyChanged();
            }
        }

        public string ProfileDescription
        {
            get => _profileDescription;
            set
            {
                _profileDescription = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<TaskManagementDataSet.RolesRow> Roles { get; set; }

        public int SelectedRoleId
        {
            get => _selectedRoleId;
            set
            {
                _selectedRoleId = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddUserCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler UserAdded;

        public UserAddViewModel(string currentUserLogin)
        {
            _dataSet = new TaskManagementDataSet();
            _usersTableAdapter = new UsersTableAdapter();
            _accountDataTableAdapter = new AccountDataTableAdapter();
            _rolesTableAdapter = new RolesTableAdapter();
            _userActivityLogger = new UserActivityLogger(ConfigurationManager.ConnectionStrings["TaskManagement.Properties.Settings.TaskManagementConnectionString"].ConnectionString);
            _currentUserLogin = currentUserLogin;

            LoadRoles();
            LoadUsers();

            AddUserCommand = new RelayCommand(AddUser);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void LoadRoles()
        {
            _rolesTableAdapter.Fill(_dataSet.Roles);
            Roles = new ObservableCollection<TaskManagementDataSet.RolesRow>(_dataSet.Roles.Cast<TaskManagementDataSet.RolesRow>());
        }

        private void LoadUsers()
        {
            _usersTableAdapter.Fill(_dataSet.Users);
        }

        private bool IsLoginUnique(string login)
        {
            return !_dataSet.Users.Any(u => u.Login == login.Replace(" ", ""));
        }

        private void AddUser(object parameter)
        {
            try
            {
                // Валидация данных
                if (!FieldValidator.IsValidName(FirstName))
                {
                    MessageBox.Show("Имя должно содержать только буквы и не менее двух символов без пробелов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!FieldValidator.IsValidName(LastName))
                {
                    MessageBox.Show("Фамилия должна содержать только буквы и не менее двух символов без пробелов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!FieldValidator.IsValidLogin(Login, IsLoginUnique))
                {
                    MessageBox.Show("Логин должен содержать буквы и цифры, быть уникальным и не менее трех символов без пробелов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (Password == null || !FieldValidator.IsValidPassword(SecureStringToString(Password)))
                {
                    MessageBox.Show("Пароль должен содержать не менее пяти символов без пробелов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!FieldValidator.IsNotNullOrEmpty(PhoneNumber) || !FieldValidator.IsValidPhoneNumber(PhoneNumber))
                {
                    MessageBox.Show("Номер телефона должен быть в формате +7XXXXXXXXXX.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!FieldValidator.IsNotNullOrEmpty(ProfileDescription) || !FieldValidator.IsValidDescription(ProfileDescription))
                {
                    MessageBox.Show("Описание должно содержать минимум один символ без пробелов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (SelectedRoleId == 0)
                {
                    MessageBox.Show("Пожалуйста, выберите роль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var userRow = _dataSet.Users.NewUsersRow();
                userRow.Login = Login;

                // Хэшируем пароль с использованием того же алгоритма, что и в LoginViewModel
                userRow.Password = HashPassword(SecureStringToString(Password));

                userRow.RoleID = SelectedRoleId;
                userRow.CreatedAt = DateTime.Now;
                userRow.UpdatedAt = DateTime.Now;

                _dataSet.Users.AddUsersRow(userRow);
                _usersTableAdapter.Update(_dataSet.Users);

                var accountRow = _dataSet.AccountData.NewAccountDataRow();
                accountRow.UserID = userRow.ID;
                accountRow.FirstName = FirstName;
                accountRow.LastName = LastName;
                accountRow.PhoneNumber = PhoneNumber;
                accountRow.ProfileDescription = ProfileDescription;

                _dataSet.AccountData.AddAccountDataRow(accountRow);
                _accountDataTableAdapter.Update(_dataSet.AccountData);

                // Логирование успешного добавления пользователя
                _userActivityLogger.LogUserActivity(GetUserId(_currentUserLogin), _currentUserLogin, $"успешно добавил пользователя {Login} в таблице Users");

                MessageBox.Show("Пользователь успешно добавлен!");

                UserAdded?.Invoke(this, EventArgs.Empty);

                // Закрываем окно после успешного добавления
                Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this)?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении пользователя: {ex.Message}");
            }
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

        private byte[] HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.Unicode.GetBytes(password);
                return sha256.ComputeHash(passwordBytes);
            }
        }

        private void Cancel(object parameter)
        {
            // Закрываем окно
            Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this)?.Close();
        }

        private string SecureStringToString(SecureString value)
        {
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(value);
                return System.Runtime.InteropServices.Marshal.PtrToStringBSTR(ptr);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(ptr);
            }
        }
    }
}
