using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TaskManagement.MVVM.Views;
using TaskManagement.MVVM.Views.AdministratorPages;
using TaskManagement.MVVM.Views.AdministratorPages.BackupPage;
using TaskManagement.MVVM.Views.AdministratorPages.CommentPage;
using TaskManagement.MVVM.Views.AdministratorPages.MetadatasPage;
using TaskManagement.MVVM.Views.AdministratorPages.PrioritiesPage;
using TaskManagement.MVVM.Views.AdministratorPages.ProjectPage;
using TaskManagement.MVVM.Views.AdministratorPages.RolePage;
using TaskManagement.MVVM.Views.AdministratorPages.TaskHistoryPage;
using TaskManagement.MVVM.Views.AdministratorPages.TaskPage;
using TaskManagement.MVVM.Views.AdministratorPages.TasksDistribPage;
using TaskManagement.MVVM.Views.AdministratorPages.UserActivityLogPage;
using TaskManagement.Views;
using TaskManagement.Helpers;
using System.Configuration;
using TaskManagement.TaskManagementDataSetTableAdapters;

namespace TaskManagement.ViewModels
{
    public class MainAdminViewModel : ViewModelBase
    {
        public ICommand CloseCommand { get; }
        public ICommand MaximizeCommand { get; }
        public ICommand MinimizeCommand { get; }
        public ICommand LoadPageCommand { get; }
        public ICommand LogoutCommand { get; }

        private readonly UserActivityLogger _userActivityLogger;

        private string _userLogin;
        public string UserLogin
        {
            get => _userLogin;
            set
            {
                _userLogin = value;
                OnPropertyChanged();
            }
        }

        public MainAdminViewModel(string userLogin)
        {
            UserLogin = userLogin;
            _userActivityLogger = new UserActivityLogger(ConfigurationManager.ConnectionStrings["TaskManagement.Properties.Settings.TaskManagementConnectionString"].ConnectionString);

            CloseCommand = new RelayCommand(_ => CloseApplication());
            MaximizeCommand = new RelayCommand(_ => MaximizeWindow());
            MinimizeCommand = new RelayCommand(_ => MinimizeWindow());
            LoadPageCommand = new RelayCommand(param => LoadPage((string)param));
            LogoutCommand = new RelayCommand(_ => Logout());
        }

        private void CloseApplication()
        {
            Application.Current.Shutdown();
        }

        private void MaximizeWindow()
        {
            var mainWindow = Application.Current.MainWindow;
            if (mainWindow.WindowState == WindowState.Normal)
            {
                mainWindow.WindowState = WindowState.Maximized;
            }
            else
            {
                mainWindow.WindowState = WindowState.Normal;
            }
        }

        private void MinimizeWindow()
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void LoadPage(string pageName)
        {
            try
            {
                var mainFrame = (Frame)Application.Current.MainWindow.FindName("MainFrame");

                mainFrame.NavigationService.RemoveBackEntry();

                Page pageType;

                switch (pageName)
                {
                    case "MainPage":
                        pageType = new MainPage(UserLogin);
                        break;
                    case "UsersPage":
                        pageType = new UsersPage(UserLogin);
                        break;
                    case "RolesPage":
                        pageType = new RolesPage(UserLogin);
                        break;
                    case "StatusesPage":
                        pageType = new StatusesPage(UserLogin);
                        break;
                    case "ProjectsPage":
                        pageType = new ProjectPage(UserLogin);
                        break;
                    case "PrioritiesPage":
                        pageType = new PrioritiesPage(UserLogin);
                        break;
                    case "TasksPage":
                        pageType = new TasksPage(UserLogin);
                        break;
                    case "TaskDistribPage":
                        pageType = new TaskDistributionsPage();
                        break;
                    case "CommentsPage":
                        pageType = new CommentsPage();
                        break;
                    case "TaskHistoryPage":
                        pageType = new TaskHistoryPage(UserLogin);
                        break;
                    case "UserActivityLogPage":
                        pageType = new UserActivityLogsPage(UserLogin);
                        break;
                    case "BackupsPage":
                        pageType = new BackupsPage(UserLogin);
                        break;
                    case "MetadataPage":
                        pageType = new MetadataPage(UserLogin);
                        break;
                    default:
                        throw new ArgumentException("Unknown page name");
                }

                mainFrame.Navigate(pageType);
                mainFrame.NavigationService.RemoveBackEntry();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки страницы: {ex.Message}");
            }
        }

        private void Logout()
        {
            int userId = GetUserId(UserLogin);
            _userActivityLogger.LogUserActivity(userId, UserLogin, "вышел из системы");

            Application.Current.MainWindow.Hide();
            var loginWindow = new LoginWindow();
            Application.Current.MainWindow = loginWindow;
            loginWindow.Show();
        }

        private int GetUserId(string login)
        {
            TaskManagementDataSet.UsersDataTable usersTable = new TaskManagementDataSet.UsersDataTable();
            UsersTableAdapter usersAdapter = new UsersTableAdapter();
            usersAdapter.Fill(usersTable);

            var userRows = usersTable.Select($"Login = '{login}'");
            var userRow = userRows.Length > 0 ? (TaskManagementDataSet.UsersRow)userRows[0] : null;
            return userRow?.ID ?? 0;
        }
    }
}
