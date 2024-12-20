using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TaskManagement.Models;
using TaskManagement.TaskManagementDataSetTableAdapters;
using TaskManagement.ViewModels;

namespace TaskManagement.MVVM.ViewModels.AdministratorPageViewModel
{
    public class TaskAddViewModel : ViewModelBase
    {
        private TaskManagementDataSet _dataSet;
        private TasksTableAdapter _tasksTableAdapter;
        private StatusesTableAdapter _statusesTableAdapter;
        private ProjectsTableAdapter _projectsTableAdapter;
        private PrioritiesTableAdapter _prioritiesTableAdapter;
        private UsersTableAdapter _usersTableAdapter;

        private string _taskTitle;
        private string _taskDescription;
        private DateTime? _dueDate;
        private int _selectedStatusId;
        private int _selectedProjectId;
        private int _selectedPriorityId;
        private string _currentUserLogin;
        private int _currentUserId;

        public string TaskTitle
        {
            get => _taskTitle;
            set
            {
                _taskTitle = value;
                OnPropertyChanged();
            }
        }

        public string TaskDescription
        {
            get => _taskDescription;
            set
            {
                _taskDescription = value;
                OnPropertyChanged();
            }
        }

        public DateTime? DueDate
        {
            get => _dueDate;
            set
            {
                _dueDate = value;
                OnPropertyChanged();
            }
        }

        public int SelectedStatusId
        {
            get => _selectedStatusId;
            set
            {
                _selectedStatusId = value;
                OnPropertyChanged();
            }
        }

        public int SelectedProjectId
        {
            get => _selectedProjectId;
            set
            {
                _selectedProjectId = value;
                OnPropertyChanged();
            }
        }

        public int SelectedPriorityId
        {
            get => _selectedPriorityId;
            set
            {
                _selectedPriorityId = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<TaskManagementDataSet.StatusesRow> Statuses { get; set; }
        public ObservableCollection<TaskManagementDataSet.ProjectsRow> Projects { get; set; }
        public ObservableCollection<TaskManagementDataSet.PrioritiesRow> Priorities { get; set; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler TaskAdded;

        public TaskAddViewModel(string currentUserLogin)
        {
            _dataSet = new TaskManagementDataSet();
            _tasksTableAdapter = new TasksTableAdapter();
            _statusesTableAdapter = new StatusesTableAdapter();
            _projectsTableAdapter = new ProjectsTableAdapter();
            _prioritiesTableAdapter = new PrioritiesTableAdapter();
            _usersTableAdapter = new UsersTableAdapter();

            _currentUserLogin = currentUserLogin;
            _currentUserId = GetUserIdByLogin(_currentUserLogin);

            LoadComboBoxData();

            SaveCommand = new RelayCommand(SaveTask);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void LoadComboBoxData()
        {
            _statusesTableAdapter.Fill(_dataSet.Statuses);
            _projectsTableAdapter.Fill(_dataSet.Projects);
            _prioritiesTableAdapter.Fill(_dataSet.Priorities);

            Statuses = new ObservableCollection<TaskManagementDataSet.StatusesRow>(_dataSet.Statuses.Cast<TaskManagementDataSet.StatusesRow>());
            Projects = new ObservableCollection<TaskManagementDataSet.ProjectsRow>(_dataSet.Projects.Cast<TaskManagementDataSet.ProjectsRow>());
            Priorities = new ObservableCollection<TaskManagementDataSet.PrioritiesRow>(_dataSet.Priorities.Cast<TaskManagementDataSet.PrioritiesRow>());
        }

        private int GetUserIdByLogin(string userLogin)
        {
            var userRow = _usersTableAdapter.GetData().FirstOrDefault(u => u.Login == userLogin);
            return userRow?.ID ?? 0;
        }

        private void SaveTask(object parameter)
        {
            try
            {
           
                if (string.IsNullOrWhiteSpace(TaskTitle))
                {
                    MessageBox.Show("Заголовок задачи не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (DueDate == null)
                {
                    MessageBox.Show("Пожалуйста, выберите срок выполнения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

          
                var newTaskRow = _dataSet.Tasks.NewTasksRow();
                newTaskRow.Title = TaskTitle;
                newTaskRow.Description = TaskDescription;
                newTaskRow.CreationDate = DateTime.Now;
                newTaskRow.DueDate = DueDate.Value;
                newTaskRow.StatusID = SelectedStatusId;
                newTaskRow.ProjectID = SelectedProjectId;
                newTaskRow.PriorityID = SelectedPriorityId;
                newTaskRow.CreatedBy = _currentUserId;
                newTaskRow.CreatedAt = DateTime.Now;
                newTaskRow.UpdatedAt = DateTime.Now;

                _dataSet.Tasks.AddTasksRow(newTaskRow);
                _tasksTableAdapter.Update(_dataSet.Tasks);

                MessageBox.Show("Задача успешно добавлена!");

                TaskAdded?.Invoke(this, EventArgs.Empty);

        
                Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this)?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении задачи: {ex.Message}");
            }
        }

        private void Cancel(object parameter)
        {
      
            Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this)?.Close();
        }
    }
}
