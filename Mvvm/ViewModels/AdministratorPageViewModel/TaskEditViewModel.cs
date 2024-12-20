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
    public class TaskEditViewModel : ViewModelBase
    {
        private TaskManagementDataSet _dataSet;
        private TasksTableAdapter _tasksTableAdapter;
        private StatusesTableAdapter _statusesTableAdapter;
        private ProjectsTableAdapter _projectsTableAdapter;
        private PrioritiesTableAdapter _prioritiesTableAdapter;

        private int _taskId;
        private string _taskTitle;
        private string _taskDescription;
        private DateTime _taskCreationDate;
        private DateTime _taskDueDate;
        private int _selectedStatusId;
        private int _selectedProjectId;
        private int _selectedPriorityId;
        private string _taskCreatedBy;
        private DateTime _taskUpdatedAt;

        public int TaskId
        {
            get => _taskId;
            private set
            {
                _taskId = value;
                OnPropertyChanged();
            }
        }

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

        public DateTime TaskCreationDate
        {
            get => _taskCreationDate;
            set
            {
                _taskCreationDate = value;
                OnPropertyChanged();
            }
        }

        public DateTime TaskDueDate
        {
            get => _taskDueDate;
            set
            {
                _taskDueDate = value;
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

        public string TaskCreatedBy
        {
            get => _taskCreatedBy;
            set
            {
                _taskCreatedBy = value;
                OnPropertyChanged();
            }
        }

        public DateTime TaskUpdatedAt
        {
            get => _taskUpdatedAt;
            set
            {
                _taskUpdatedAt = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<TaskManagementDataSet.StatusesRow> Statuses { get; set; }
        public ObservableCollection<TaskManagementDataSet.ProjectsRow> Projects { get; set; }
        public ObservableCollection<TaskManagementDataSet.PrioritiesRow> Priorities { get; set; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler TaskUpdated;

        public TaskEditViewModel(int taskId)
        {
            _dataSet = new TaskManagementDataSet();
            _tasksTableAdapter = new TasksTableAdapter();
            _statusesTableAdapter = new StatusesTableAdapter();
            _projectsTableAdapter = new ProjectsTableAdapter();
            _prioritiesTableAdapter = new PrioritiesTableAdapter();

            TaskId = taskId;

            LoadTask(taskId);
            LoadComboBoxData();

            SaveCommand = new RelayCommand(SaveTask);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void LoadTask(int taskId)
        {
            _tasksTableAdapter.Fill(_dataSet.Tasks);
            var taskRow = _dataSet.Tasks.FindByID(taskId);

            if (taskRow != null)
            {
                TaskTitle = taskRow.Title;
                TaskDescription = taskRow.Description;
                TaskCreationDate = taskRow.CreationDate;
                TaskDueDate = taskRow.DueDate;
                SelectedStatusId = taskRow.StatusID;
                SelectedProjectId = taskRow.ProjectID;
                SelectedPriorityId = taskRow.PriorityID;
                TaskCreatedBy = taskRow.CreatedBy.ToString();
                TaskUpdatedAt = taskRow.UpdatedAt;
            }
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

        private void SaveTask(object parameter)
        {
            try
            {
     
                if (string.IsNullOrWhiteSpace(TaskTitle))
                {
                    MessageBox.Show("Заголовок задачи не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

         
                var taskRow = _dataSet.Tasks.FindByID(TaskId);

                if (taskRow != null)
                {
                    taskRow.Title = TaskTitle;
                    taskRow.Description = TaskDescription;
                    taskRow.CreationDate = TaskCreationDate;
                    taskRow.DueDate = TaskDueDate;
                    taskRow.StatusID = SelectedStatusId;
                    taskRow.ProjectID = SelectedProjectId;
                    taskRow.PriorityID = SelectedPriorityId;
                    taskRow.UpdatedAt = DateTime.Now;

                    _tasksTableAdapter.Update(_dataSet.Tasks);

                    MessageBox.Show("Задача успешно обновлена!");

                    TaskUpdated?.Invoke(this, EventArgs.Empty);

        
                    Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this)?.Close();
                }
                else
                {
                    MessageBox.Show("Задача не найдена.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении задачи: {ex.Message}");
            }
        }

        private void Cancel(object parameter)
        {
   
            Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this)?.Close();
        }
    }
}
