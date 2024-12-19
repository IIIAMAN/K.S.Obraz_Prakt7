using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;
using TaskManagement.Models;
using TaskManagement.TaskManagementDataSetTableAdapters;
using TaskManagement.Views;
using TaskManagement.Helpers;
using System.Configuration;
using TaskManagement.MVVM.Views;

namespace TaskManagement.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private User _user;
        private string _userPassword;
        private string _errorMessage;
        private readonly UserActivityLogger _userActivityLogger;

        public event PropertyChangedEventHandler PropertyChanged;

        public string UserLogin
        {
            get => _user.Login;
            set
            {
                if (_user.Login != value)
                {
                    _user.Login = value;
                    OnPropertyChanged();
                }
            }
        }

        public string UserPassword
        {
            get => _userPassword;
            set
            {
                if (_userPassword != value)
                {
                    _userPassword = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (_errorMessage != value)
                {
                    _errorMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand LoginCommand { get; }

        public LoginViewModel()
        {
            _user = new User();
            _userActivityLogger = new UserActivityLogger(ConfigurationManager.ConnectionStrings["TaskManagement.Properties.Settings.TaskManagementConnectionString"].ConnectionString);
            LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
        }

        private void ExecuteLogin(object parameter)
        {
            if (string.IsNullOrWhiteSpace(UserLogin) || string.IsNullOrWhiteSpace(UserPassword))
            {
                ErrorMessage = "Пожалуйста, заполните все поля.";
                MessageBox.Show("Заполните все поля");

                // Логируем попытку входа с пустыми полями
                LogUserActivity(0, UserLogin, "попытка входа с пустыми полями");
                return;
            }

            // Проверяем логин и пароль через базу данных
            bool isValidUser = AuthenticateUser(UserLogin, UserPassword);

            if (isValidUser)
            {
                ErrorMessage = string.Empty;
                MessageBox.Show("Авторизация прошла успешно");

                // Логируем успешный вход
                int userId = GetUserId(UserLogin); // Получение корректного UserID
                LogUserActivity(userId, UserLogin, "успешный вход в систему");
            }
            else
            {
                ErrorMessage = "Неверный логин или пароль.";
                MessageBox.Show("Неверный логин или пароль");

                // Логируем неуспешную попытку входа
                LogUserActivity(0, UserLogin, "неуспешная попытка входа");
            }
        }

        private void LogUserActivity(int userId, string login, string action)
        {
            if (userId > 0)
            {
                _userActivityLogger.LogUserActivity(userId, login, action);
            }
            else
            {
                // Логируем действия без использования UserActivityLogger при userId == 0
                Console.WriteLine($"{DateTime.Now}: {login} - {action}");
            }
        }

        private int GetUserId(string login)
        {
            var userRow = GetUserFromDatabase(login);
            return userRow?.ID ?? 0; // Получаем ID пользователя (если найден)
        }

        private bool CanExecuteLogin(object parameter)
        {
            return !string.IsNullOrWhiteSpace(UserLogin) && !string.IsNullOrWhiteSpace(UserPassword);
        }

        private bool AuthenticateUser(string login, string password)
        {
            try
            {
                var userRow = GetUserFromDatabase(login);
                if (userRow == null)
                {
                    ErrorMessage = "Пользователь с таким логином не найден.";
                    Console.WriteLine("Пользователь не найден");
                    return false;
                }

                // Получаем хэшированный пароль, сохраненный в базе данных (в бинарном формате)
                byte[] storedPasswordBinary = userRow.Password;

                // Хэшируем введенный пароль
                byte[] hashedPasswordBinary = HashPasswordBinary(password);

                // Логирование для отладки
                Console.WriteLine($"Login: {login}");
                Console.WriteLine($"Password (Input): {password}");
                Console.WriteLine($"Hashed Password (Input): {BitConverter.ToString(hashedPasswordBinary)}");
                Console.WriteLine($"Stored Password (DB): {BitConverter.ToString(storedPasswordBinary)}");

                // Сравниваем хэшированные пароли
                if (!hashedPasswordBinary.SequenceEqual(storedPasswordBinary))
                {
                    ErrorMessage = "Неверный пароль.";
                    Console.WriteLine("Неверный пароль");
                    return false;
                }

                string roleName = GetRoleNameById(userRow.RoleID);
                OpenPageBasedOnRole(roleName, login);

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = "Ошибка при проверке пользователя: " + ex.Message;
                Console.WriteLine("Ошибка: " + ex.Message);
                return false;
            }
        }

        private byte[] HashPasswordBinary(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.Unicode.GetBytes(password);
                return sha256.ComputeHash(passwordBytes);
            }
        }

        private TaskManagementDataSet.UsersRow GetUserFromDatabase(string login)
        {
            TaskManagementDataSet.UsersDataTable usersTable = new TaskManagementDataSet.UsersDataTable();
            UsersTableAdapter usersAdapter = new UsersTableAdapter();
            usersAdapter.Fill(usersTable);

            var userRows = usersTable.Select($"Login = '{login}'");
            return userRows.Length > 0 ? (TaskManagementDataSet.UsersRow)userRows[0] : null;
        }

        private string GetRoleNameById(int roleId)
        {
            TaskManagementDataSet.RolesDataTable rolesTable = new TaskManagementDataSet.RolesDataTable();
            RolesTableAdapter rolesAdapter = new RolesTableAdapter();
            rolesAdapter.Fill(rolesTable);

            var roleRows = rolesTable.Select($"Id = {roleId}");
            if (roleRows.Length == 0)
            {
                throw new Exception("Роль с указанным ID не найдена.");
            }

            var roleRow = (TaskManagementDataSet.RolesRow)roleRows[0];
            return roleRow.Name;
        }

        private void OpenPageBasedOnRole(string roleName, string login)
        {
            switch (roleName)
            {
                case "TaskManager":
                    OpenTaskManagerPage(login);
                    break;
                case "TaskExecutor":
                    OpenTaskExecutorPage(login);
                    break;
                case "Administrator":
                    OpenAdminPage(login);
                    break;
                case "ChiefTaskExecutor":
                    OpenChiefTaskExecutorPage(login);
                    break;
                default:
                    throw new Exception("Неизвестная роль пользователя.");
            }
        }

        private void OpenTaskManagerPage(string login)
        {
            try
            {
                Console.WriteLine("Попытка открытия окна TaskManager...");

                // Создаем экземпляр окна с передачей логина
                MainTaskManagerWindow mainTaskManagerWindow = new MainTaskManagerWindow(login);

                // Закрываем текущее окно
                Application.Current.MainWindow.Close();

                // Устанавливаем новое окно как главное
                Application.Current.MainWindow = mainTaskManagerWindow;

                Console.WriteLine("Окно TaskManager успешно создано.");
                mainTaskManagerWindow.Show();
                Console.WriteLine("Окно TaskManager успешно открыто.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при открытии окна TaskManager: {ex.Message}");
            }
        }

        private void OpenTaskExecutorPage(string login)
        {
            try
            {
                Console.WriteLine("Попытка открытия окна TaskExecutor...");

                // Создаем экземпляр окна с передачей логина
                MainTaskExecutorWindow mainTaskExecutorWindow = new MainTaskExecutorWindow(login);

                // Закрываем текущее окно
                Application.Current.MainWindow.Close();

                // Устанавливаем новое окно как главное
                Application.Current.MainWindow = mainTaskExecutorWindow;

                Console.WriteLine("Окно TaskExecutor успешно создано.");
                mainTaskExecutorWindow.Show();
                Console.WriteLine("Окно TaskExecutor успешно открыто.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при открытии окна TaskExecutor: {ex.Message}");
            }
        }

        private void OpenAdminPage(string userLogin)
        {
            try
            {
                Console.WriteLine("Попытка открытия окна администратора...");

                MainAdminWindow mainAdminWindow = new MainAdminWindow(userLogin);

                Application.Current.MainWindow.Close();
                Application.Current.MainWindow = mainAdminWindow;

                Console.WriteLine("Окно администратора успешно создано.");
                mainAdminWindow.Show();
                Console.WriteLine("Окно администратора успешно открыто.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при открытии окна администратора: {ex.Message}");
            }
        }

        private void OpenChiefTaskExecutorPage(string login)
        {
            try
            {
                Console.WriteLine("Попытка открытия окна главного исполнителя задач...");

                MainChiefTaskExecutorWindow mainChiefTaskExecutorWindow = new MainChiefTaskExecutorWindow(login);

                Application.Current.MainWindow.Close();
                Application.Current.MainWindow = mainChiefTaskExecutorWindow;

                Console.WriteLine("Окно главного исполнителя задач успешно создано.");
                mainChiefTaskExecutorWindow.Show();
                Console.WriteLine("Окно главного исполнителя задач успешно открыто.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при открытии окна главного исполнителя задач: {ex.Message}");
            }
        }



        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
