using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TaskManagement.Models;
using TaskManagement.MVVM.Views.AdministratorPages.TaskPage;
using TaskManagement.MVVM.Views.AdministratorPages.TasksPage;
using TaskManagement.TaskManagementDataSetTableAdapters;
using TaskManagement.ViewModels;

namespace TaskManagement.MVVM.ViewModels.AdministratorPageViewModel
{
    public class TasksPageViewModel : ViewModelBase
    {
        private TaskManagementDataSet _dataSet;
        private TasksTableAdapter _tasksTableAdapter;
        private ProjectsTableAdapter _projectsTableAdapter;
        private StatusesTableAdapter _statusesTableAdapter;
        private UsersTableAdapter _usersTableAdapter;
        private PrioritiesTableAdapter _prioritiesTableAdapter;
        private ObservableCollection<dynamic> _tasks;
        private dynamic _selectedTask;
        private string _currentUserLogin;

        public ObservableCollection<dynamic> Tasks
        {
            get => _tasks;
            set
            {
                _tasks = value;
                OnPropertyChanged();
            }
        }

        public dynamic SelectedTask
        {
            get => _selectedTask;
            set
            {
                _selectedTask = value;
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

        public ICommand AddTaskCommand { get; }
        public ICommand EditTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }

        public TasksPageViewModel()
        {
            _dataSet = new TaskManagementDataSet();
            _tasksTableAdapter = new TasksTableAdapter();
            _projectsTableAdapter = new ProjectsTableAdapter();
            _statusesTableAdapter = new StatusesTableAdapter();
            _usersTableAdapter = new UsersTableAdapter();
            _prioritiesTableAdapter = new PrioritiesTableAdapter();

            LoadTasks();

            AddTaskCommand = new RelayCommand(AddTask);
            EditTaskCommand = new RelayCommand(EditTask, CanEditOrDeleteTask);
            DeleteTaskCommand = new RelayCommand(DeleteTask, CanEditOrDeleteTask);
        }

        public void LoadTasks()
        {
            _tasksTableAdapter.Fill(_dataSet.Tasks);
            _projectsTableAdapter.Fill(_dataSet.Projects);
            _statusesTableAdapter.Fill(_dataSet.Statuses);
            _usersTableAdapter.Fill(_dataSet.Users);
            _prioritiesTableAdapter.Fill(_dataSet.Priorities);

            Tasks = new ObservableCollection<dynamic>(_dataSet.Tasks
                .Cast<TaskManagementDataSet.TasksRow>()
                .Select(task => new
                {
                    task.ID,
                    task.Title,
                    task.Description,
                    CreationDate = task.CreationDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    DueDate = task.IsDueDateNull() ? "нет срока" : task.DueDate.ToString("yyyy-MM-dd HH:mm:ss"),
                    DaysRemaining = task.IsDueDateNull() ? "нет срока" : (task.DueDate - DateTime.Now).Days + " дней осталось",
                    StatusName = _dataSet.Statuses.FirstOrDefault(s => s.ID == task.StatusID)?.Name,
                    ProjectName = _dataSet.Projects.FirstOrDefault(p => p.ID == task.ProjectID)?.Name,
                    PriorityName = _dataSet.Priorities.FirstOrDefault(pr => pr.ID == task.PriorityID)?.Name,
                    CreatedBy = _dataSet.Users.FirstOrDefault(u => u.ID == task.CreatedBy)?.Login,
                    CreatedAt = task.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    UpdatedAt = task.IsUpdatedAtNull() ? "нет обновления" : task.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                }).ToList());
        }

        private void AddTask(object parameter)
        {
            var addTaskWindow = new TaskAddWindow(CurrentUserLogin);
            var viewModel = (TaskAddViewModel)addTaskWindow.DataContext;

            viewModel.TaskAdded += (s, args) => LoadTasks();

            addTaskWindow.ShowDialog();
        }

        private void EditTask(object parameter)
        {
            if (SelectedTask != null)
            {
                var editWindow = new TaskEditWindow(SelectedTask.ID);
                var viewModel = (TaskEditViewModel)editWindow.DataContext;

                viewModel.TaskUpdated += (s, args) => LoadTasks();

                if (editWindow.ShowDialog() == true)
                {
                    LoadTasks();
                }
            }
        }

        private void DeleteTask(object parameter)
        {
            if (SelectedTask != null)
            {
                if (MessageBox.Show("Вы уверены, что хотите удалить эту задачу?", "Удаление задачи", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        var taskRow = _dataSet.Tasks.FindByID(SelectedTask.ID);
                        if (taskRow != null)
                        {
                            taskRow.Delete();
                            _tasksTableAdapter.Update(_dataSet.Tasks);
                            LoadTasks();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении задачи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private bool CanEditOrDeleteTask(object parameter)
        {
            return SelectedTask != null;
        }
    }
}
