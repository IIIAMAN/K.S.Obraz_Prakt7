using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TaskManagement.Helpers; // Пространство имен для FieldValidator
using TaskManagement.Models;
using TaskManagement.TaskManagementDataSetTableAdapters;
using TaskManagement.ViewModels;
using System.Configuration;

namespace TaskManagement.MVVM.ViewModels.AdministratorPageViewModel
{
    public class ProjectAddViewModel : ViewModelBase
    {
        private TaskManagementDataSet _dataSet;
        private ProjectsTableAdapter _projectsTableAdapter;
        private readonly UserActivityLogger _userActivityLogger;
        private string _currentUserLogin;

        private string _projectName;
        private string _projectDescription;

        public string ProjectName
        {
            get => _projectName;
            set
            {
                _projectName = value;
                OnPropertyChanged();
            }
        }

        public string ProjectDescription
        {
            get => _projectDescription;
            set
            {
                _projectDescription = value;
                OnPropertyChanged();
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler ProjectAdded;

        public ProjectAddViewModel(string currentUserLogin)
        {
            _dataSet = new TaskManagementDataSet();
            _projectsTableAdapter = new ProjectsTableAdapter();
            _userActivityLogger = new UserActivityLogger(ConfigurationManager.ConnectionStrings["TaskManagement.Properties.Settings.TaskManagementConnectionString"].ConnectionString);
            _currentUserLogin = currentUserLogin;

            SaveCommand = new RelayCommand(SaveProject);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void SaveProject(object parameter)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ProjectName))
                {
                    MessageBox.Show("Название проекта не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var existingProject = _dataSet.Projects.FirstOrDefault(p => p.Name == ProjectName);
                if (existingProject != null)
                {
                    MessageBox.Show("Проект с таким именем уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var newProjectRow = _dataSet.Projects.NewProjectsRow();
                newProjectRow.Name = ProjectName;
                newProjectRow.Description = ProjectDescription;
                newProjectRow.CreatedAt = DateTime.Now;
                newProjectRow.UpdatedAt = DateTime.Now;

                _dataSet.Projects.AddProjectsRow(newProjectRow);
                _projectsTableAdapter.Update(_dataSet.Projects);

                // Логирование успешного добавления проекта
                _userActivityLogger.LogUserActivity(GetUserId(_currentUserLogin), _currentUserLogin, $"успешно добавил проект с именем {ProjectName}");

                MessageBox.Show("Проект успешно добавлен!");

                ProjectAdded?.Invoke(this, EventArgs.Empty);

                Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this)?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении проекта: {ex.Message}");
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
