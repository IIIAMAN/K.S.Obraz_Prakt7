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
    public class TaskDistribAddViewModel : ViewModelBase
    {
        private TaskManagementDataSet _dataSet;
        private TaskDistributionTableAdapter _taskDistribsTableAdapter;
        private TasksTableAdapter _tasksTableAdapter;
        private UsersTableAdapter _usersTableAdapter;

        private int _selectedTaskId;
        private int _selectedUserId;

        public int SelectedTaskId
        {
            get => _selectedTaskId;
            set
            {
                _selectedTaskId = value;
                OnPropertyChanged();
            }
        }

        public int SelectedUserId
        {
            get => _selectedUserId;
            set
            {
                _selectedUserId = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<TaskManagementDataSet.TasksRow> Tasks { get; set; }
        public ObservableCollection<TaskManagementDataSet.UsersRow> Users { get; set; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler DistribAdded;

        public TaskDistribAddViewModel()
        {
            _dataSet = new TaskManagementDataSet();
            _taskDistribsTableAdapter = new TaskDistributionTableAdapter();
            _tasksTableAdapter = new TasksTableAdapter();
            _usersTableAdapter = new UsersTableAdapter();

            LoadComboBoxData();

            SaveCommand = new RelayCommand(SaveDistrib);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void LoadComboBoxData()
        {
            _tasksTableAdapter.Fill(_dataSet.Tasks);
            _usersTableAdapter.Fill(_dataSet.Users);

            Tasks = new ObservableCollection<TaskManagementDataSet.TasksRow>(_dataSet.Tasks.Cast<TaskManagementDataSet.TasksRow>());
            Users = new ObservableCollection<TaskManagementDataSet.UsersRow>(_dataSet.Users.Cast<TaskManagementDataSet.UsersRow>());
        }

        private void SaveDistrib(object parameter)
        {
            try
            {
    
                Console.WriteLine($"SelectedTaskId: {SelectedTaskId}, SelectedUserId: {SelectedUserId}");

         
                if (SelectedTaskId == 0 || SelectedUserId == 0)
                {
                    MessageBox.Show("Пожалуйста, выберите задачу и пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

       
                var newDistribRow = _dataSet.TaskDistribution.NewTaskDistributionRow();
                newDistribRow.TaskID = SelectedTaskId;
                newDistribRow.UserID = SelectedUserId;
                newDistribRow.AssignedDate = DateTime.Now;

                _dataSet.TaskDistribution.AddTaskDistributionRow(newDistribRow);
                _taskDistribsTableAdapter.Update(_dataSet.TaskDistribution);

                MessageBox.Show("Распределение успешно добавлено!");

                DistribAdded?.Invoke(this, EventArgs.Empty);

          
                Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this)?.Close();
            }
            catch (Exception ex)
            {
     
                Console.WriteLine($"Ошибка при добавлении распределения: {ex.Message}");
                MessageBox.Show($"Ошибка при добавлении распределения: {ex.Message}");
            }
        }

        private void Cancel(object parameter)
        {
        
            Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this)?.Close();
        }
    }
}
