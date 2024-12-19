using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows;
using System;
using System.Linq;
using TaskManagement.MVVM.Views.AdministratorPages.ProjectPage;
using TaskManagement.TaskManagementDataSetTableAdapters;
using TaskManagement.ViewModels;
using TaskManagement.Helpers;
using System.Configuration;

namespace TaskManagement.MVVM.ViewModels.AdministratorPageViewModel
{
    public class ProjectsPageViewModel : ViewModelBase
    {
        private TaskManagementDataSet _dataSet;
        private ProjectsTableAdapter _projectsTableAdapter;
        private ObservableCollection<dynamic> _projects;
        private dynamic _selectedProject;
        private string _currentUserLogin;
        private readonly UserActivityLogger _userActivityLogger; // Поле для логгера

        public ObservableCollection<dynamic> Projects
        {
            get => _projects;
            set
            {
                _projects = value;
                OnPropertyChanged();
            }
        }

        public dynamic SelectedProject
        {
            get => _selectedProject;
            set
            {
                _selectedProject = value;
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

        public ICommand AddProjectCommand { get; }
        public ICommand EditProjectCommand { get; }
        public ICommand DeleteProjectCommand { get; }

        public ProjectsPageViewModel()
        {
            _dataSet = new TaskManagementDataSet();
            _projectsTableAdapter = new ProjectsTableAdapter();
            _userActivityLogger = new UserActivityLogger(ConfigurationManager.ConnectionStrings["TaskManagement.Properties.Settings.TaskManagementConnectionString"].ConnectionString); // Инициализация логгера

            LoadProjects();

            AddProjectCommand = new RelayCommand(AddProject);
            EditProjectCommand = new RelayCommand(EditProject, CanEditOrDeleteProject);
            DeleteProjectCommand = new RelayCommand(DeleteProject, CanEditOrDeleteProject);
        }

        public void LoadProjects()
        {
            _projectsTableAdapter.Fill(_dataSet.Projects);
            Projects = new ObservableCollection<dynamic>(_dataSet.Projects
                .Cast<TaskManagementDataSet.ProjectsRow>()
                .Select(pr => new
                {
                    pr.ID,
                    pr.Name,
                    pr.Description,
                    CreatedAt = pr.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    UpdatedAt = pr.IsUpdatedAtNull() ? "null" : pr.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                }).ToList());
        }

        private void AddProject(object parameter)
        {
            var addProjectWindow = new ProjectAddWindow(CurrentUserLogin); // Передаем логин
            var viewModel = (ProjectAddViewModel)addProjectWindow.DataContext;

            viewModel.ProjectAdded += (s, args) => LoadProjects();

            addProjectWindow.ShowDialog();
        }

        private void EditProject(object parameter)
        {
            if (SelectedProject != null)
            {
                var editWindow = new ProjectEditWindow(SelectedProject.ID, CurrentUserLogin); // Передаем ID проекта и логин пользователя
                var viewModel = (ProjectEditViewModel)editWindow.DataContext;

                viewModel.ProjectUpdated += (s, args) => LoadProjects();

                if (editWindow.ShowDialog() == true)
                {
                    LoadProjects(); // Обновляем данные после редактирования
                }
            }
        }

        private void DeleteProject(object parameter)
        {
            if (SelectedProject != null)
            {
                if (MessageBox.Show("Вы уверены, что хотите удалить этот проект?", "Удаление проекта", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        var projectRow = _dataSet.Projects.FindByID(SelectedProject.ID);
                        if (projectRow != null)
                        {
                            projectRow.Delete();
                            _projectsTableAdapter.Update(_dataSet.Projects);
                            LoadProjects(); // Обновляем данные после удаления

                            // Логирование удаления
                            _userActivityLogger.LogUserActivity(GetUserId(CurrentUserLogin), CurrentUserLogin, $"удалил проект");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении проекта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private bool CanEditOrDeleteProject(object parameter)
        {
            return SelectedProject != null;
        }

        private int GetUserId(string login)
        {
            var userRow = _dataSet.Users.FirstOrDefault(u => u.Login == login);
            if (userRow == null)
            {
                MessageBox.Show($"Пользователь с логином {login} не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                _userActivityLogger.LogUserActivity(0, login, "Пользователь с указанным логином не найден.");
            }
            return userRow?.ID ?? 0;
        }
    }
}
