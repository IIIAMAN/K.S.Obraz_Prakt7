using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TaskManagement.Models;
using TaskManagement.MVVM.Views.AdministratorPages.TasksDistribPage;
using TaskManagement.TaskManagementDataSetTableAdapters;
using TaskManagement.ViewModels;

namespace TaskManagement.MVVM.ViewModels.AdministratorPageViewModel
{
    public class TaskDistributionsPageViewModel : ViewModelBase
    {
        private TaskManagementDataSet _dataSet;
        private TaskDistributionTableAdapter _taskDistribsTableAdapter;
        private TasksTableAdapter _tasksTableAdapter;
        private UsersTableAdapter _usersTableAdapter;
        private ObservableCollection<dynamic> _taskDistributions;
        private ObservableCollection<dynamic> _tasks;
        private dynamic _selectedTaskDistribution;

        public ObservableCollection<dynamic> TaskDistributions
        {
            get => _taskDistributions;
            set
            {
                _taskDistributions = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<dynamic> Tasks
        {
            get => _tasks;
            set
            {
                _tasks = value;
                OnPropertyChanged();
            }
        }

        public dynamic SelectedTaskDistribution
        {
            get => _selectedTaskDistribution;
            set
            {
                _selectedTaskDistribution = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddDistribCommand { get; }
        public ICommand EditDistribCommand { get; }
        public ICommand DeleteDistribCommand { get; }

        public string CurrentUserLogin { get; set; }

        public TaskDistributionsPageViewModel()
        {
            _dataSet = new TaskManagementDataSet();
            _taskDistribsTableAdapter = new TaskDistributionTableAdapter();
            _tasksTableAdapter = new TasksTableAdapter();
            _usersTableAdapter = new UsersTableAdapter();

            LoadTaskDistributions();
            LoadTasks();

            AddDistribCommand = new RelayCommand(AddDistrib);
            EditDistribCommand = new RelayCommand(EditDistrib, CanEditOrDeleteDistrib);
            DeleteDistribCommand = new RelayCommand(DeleteDistrib, CanEditOrDeleteDistrib);
        }

        public void LoadTaskDistributions()
        {
            _taskDistribsTableAdapter.Fill(_dataSet.TaskDistribution);
            _tasksTableAdapter.Fill(_dataSet.Tasks);
            _usersTableAdapter.Fill(_dataSet.Users);

            TaskDistributions = new ObservableCollection<dynamic>(_dataSet.TaskDistribution
                .Cast<TaskManagementDataSet.TaskDistributionRow>()
                .Select(distrib => new
                {
                    distrib.ID,
                    distrib.TaskID,
                    TaskTitle = _dataSet.Tasks.FirstOrDefault(t => t.ID == distrib.TaskID)?.Title,
                    distrib.UserID,
                    UserLogin = _dataSet.Users.FirstOrDefault(u => u.ID == distrib.UserID)?.Login,
                    AssignedDate = distrib.AssignedDate.ToString("yyyy-MM-dd HH:mm:ss")
                }).ToList());
        }

        public void LoadTasks()
        {
            _tasksTableAdapter.Fill(_dataSet.Tasks);
            _usersTableAdapter.Fill(_dataSet.Users);

            Tasks = new ObservableCollection<dynamic>(_dataSet.Tasks
                .Cast<TaskManagementDataSet.TasksRow>()
                .Select(task => new
                {
                    task.ID,
                    task.Title,
                    Users = _dataSet.TaskDistribution
                        .Where(td => td.TaskID == task.ID)
                        .Select(td => new
                        {
                            UserLogin = _dataSet.Users.FirstOrDefault(u => u.ID == td.UserID)?.Login
                        }).ToList()
                }).ToList());
        }

        private void AddDistrib(object parameter)
        {
            var addDistribWindow = new TaskDistribAddWindow();
            var viewModel = (TaskDistribAddViewModel)addDistribWindow.DataContext;

            viewModel.DistribAdded += (s, args) =>
            {
                LoadTaskDistributions();
                LoadTasks();
            };

            addDistribWindow.ShowDialog();
        }

        private void EditDistrib(object parameter)
        {
            if (SelectedTaskDistribution != null)
            {
                var editWindow = new TaskDistribEditWindow(SelectedTaskDistribution.ID);
                var viewModel = (TaskDistribEditViewModel)editWindow.DataContext;

                viewModel.DistribUpdated += (s, args) =>
                {
                    LoadTaskDistributions();
                    LoadTasks();
                };

                if (editWindow.ShowDialog() == true)
                {
                    LoadTaskDistributions();
                    LoadTasks();
                }
            }
        }

        private void DeleteDistrib(object parameter)
        {
            if (SelectedTaskDistribution != null)
            {
                if (MessageBox.Show("Вы уверены, что хотите удалить это распределение?", "Удаление распределения", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        var distribRow = _dataSet.TaskDistribution.FindByID(SelectedTaskDistribution.ID);
                        if (distribRow != null)
                        {
                            distribRow.Delete();
                            _taskDistribsTableAdapter.Update(_dataSet.TaskDistribution);
                            LoadTaskDistributions();
                            LoadTasks();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении распределения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private bool CanEditOrDeleteDistrib(object parameter)
        {
            return SelectedTaskDistribution != null;
        }
    }
}
