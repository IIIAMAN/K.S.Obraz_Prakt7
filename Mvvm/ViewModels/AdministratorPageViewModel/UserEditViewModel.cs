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
using TaskManagement.ViewModels;

namespace TaskManagement.MVVM.ViewModels.AdministratorPageViewModel
{
    public class UserEditViewModel : ViewModelBase
    {
        private TaskManagementDataSet _dataSet;
        private UsersTableAdapter _usersTableAdapter;
        private AccountDataTableAdapter _accountDataTableAdapter;
        private RolesTableAdapter _rolesTableAdapter;
        private UserActivityLogger _userActivityLogger;

        public int UserId { get; private set; }
        public string CurrentUserLogin { get; set; }

        private string _login;
        private SecureString _password;
        private string _firstName;
        private string _lastName;
        private string _phoneNumber;
        private string _profileDescription;
        private int _selectedRoleId;

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

        public ICommand SaveUserCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler UserUpdated;

        public UserEditViewModel()
        {
            SaveUserCommand = new RelayCommand(SaveUser);
            CancelCommand = new RelayCommand(Cancel);

            _userActivityLogger = new UserActivityLogger(ConfigurationManager.ConnectionStrings["TaskManagement.Properties.Settings.TaskManagementConnectionString"].ConnectionString);
        }

        public void Initialize(int userId, string currentUserLogin)
        {
            _dataSet = new TaskManagementDataSet();
            _usersTableAdapter = new UsersTableAdapter();
            _accountDataTableAdapter = new AccountDataTableAdapter();
            _rolesTableAdapter = new RolesTableAdapter();

            UserId = userId;
            CurrentUserLogin = currentUserLogin;

            LoadRoles();
            LoadUser(userId);
        }

        private void LoadRoles()
        {
            _rolesTableAdapter.Fill(_dataSet.Roles);
            Roles = new ObservableCollection<TaskManagementDataSet.RolesRow>(_dataSet.Roles.Cast<TaskManagementDataSet.RolesRow>());
        }

        private void LoadUser(int userId)
        {
            _usersTableAdapter.Fill(_dataSet.Users);
            _accountDataTableAdapter.Fill(_dataSet.AccountData);

            var userRow = _dataSet.Users.FirstOrDefault(u => u.ID == userId);
            if (userRow != null)
            {
                Login = userRow.Login;
                Password = new SecureString();
                foreach (char c in Encoding.UTF8.GetString(userRow.Password))
                {
                    Password.AppendChar(c);
                }

                SelectedRoleId = userRow.RoleID;

                var accountRow = _dataSet.AccountData.FirstOrDefault(a => a.UserID == userRow.ID);
                if (accountRow != null)
                {
                    FirstName = accountRow.FirstName;
                    LastName = accountRow.LastName;
                    PhoneNumber = accountRow.PhoneNumber;
                    ProfileDescription = accountRow.ProfileDescription;
                }
            }
        }

        private bool IsLoginUnique(string login)
        {
            return !_dataSet.Users.Any(u => u.Login == login.Replace(" ", "") && u.ID != UserId);
        }

        private void SaveUser(object parameter)
        {
            try
            {
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

                var userRow = _dataSet.Users.FirstOrDefault(u => u.ID == UserId);
                if (userRow == null)
                {
                    MessageBox.Show("Пользователь не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                userRow.Login = Login;
                userRow.RoleID = SelectedRoleId;
                userRow.UpdatedAt = DateTime.Now;

                _usersTableAdapter.Update(_dataSet.Users);

                var accountRow = _dataSet.AccountData.FirstOrDefault(a => a.UserID == userRow.ID) ?? _dataSet.AccountData.NewAccountDataRow();

                accountRow.UserID = userRow.ID;
                accountRow.FirstName = FirstName;
                accountRow.LastName = LastName;
                accountRow.PhoneNumber = PhoneNumber;
                accountRow.ProfileDescription = ProfileDescription;

                if (accountRow.RowState == System.Data.DataRowState.Detached)
                {
                    _dataSet.AccountData.AddAccountDataRow(accountRow);
                }

                _accountDataTableAdapter.Update(_dataSet.AccountData);

                _userActivityLogger.LogUserActivity(GetUserId(CurrentUserLogin), CurrentUserLogin, $"успешно отредактировал пользователя {Login} в таблице Users");

                MessageBox.Show("Данные успешно сохранены!");

                UserUpdated?.Invoke(this, EventArgs.Empty);

                Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this)?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных пользователя: {ex.Message}");
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

        private byte[] HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.Unicode.GetBytes(password);
                return sha256.ComputeHash(passwordBytes);
            }
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
