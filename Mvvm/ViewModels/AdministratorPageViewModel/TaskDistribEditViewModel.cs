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
    public class TaskDistribEditViewModel : ViewModelBase
    {
        private TaskManagementDataSet _dataSet;
        private TaskDistributionTableAdapter _taskDistribsTableAdapter;
        private TasksTableAdapter _tasksTableAdapter;
        private UsersTableAdapter _usersTableAdapter;

        private int _taskDistribId;
        private int _selectedTaskId;
        private int _selectedUserId;

        public int TaskDistribId
        {
            get => _taskDistribId;
            private set
            {
                _taskDistribId = value;
                OnPropertyChanged();
            }
        }

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

        public event EventHandler DistribUpdated;

        public TaskDistribEditViewModel(int distribId)
        {
            _dataSet = new TaskManagementDataSet();
            _taskDistribsTableAdapter = new TaskDistributionTableAdapter();
            _tasksTableAdapter = new TasksTableAdapter();
            _usersTableAdapter = new UsersTableAdapter();

            TaskDistribId = distribId;

            LoadTaskDistrib(distribId);
            LoadComboBoxData();

            SaveCommand = new RelayCommand(SaveDistrib);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void LoadTaskDistrib(int distribId)
        {
            _taskDistribsTableAdapter.Fill(_dataSet.TaskDistribution);
            var distribRow = _dataSet.TaskDistribution.FindByID(distribId);

            if (distribRow != null)
            {
                SelectedTaskId = distribRow.TaskID;
                SelectedUserId = distribRow.UserID;
            }
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
                // Логирование выбранных значений
                Console.WriteLine($"SelectedTaskId: {SelectedTaskId}, SelectedUserId: {SelectedUserId}");

                // Проверяем, выбраны ли задачи и пользователи
                if (SelectedTaskId == 0 || SelectedUserId == 0)
                {
                    MessageBox.Show("Пожалуйста, выберите задачу и пользователя.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Обновление существующего распределения
                var distribRow = _dataSet.TaskDistribution.FindByID(TaskDistribId);

                if (distribRow != null)
                {
                    distribRow.TaskID = SelectedTaskId;
                    distribRow.UserID = SelectedUserId;
                    distribRow.AssignedDate = DateTime.Now;

                    _taskDistribsTableAdapter.Update(_dataSet.TaskDistribution);

                    MessageBox.Show("Распределение успешно обновлено!");

                    DistribUpdated?.Invoke(this, EventArgs.Empty);

                    // Закрытие окна
                    Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this)?.Close();
                }
                else
                {
                    MessageBox.Show("Распределение не найдено.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                Console.WriteLine($"Ошибка при обновлении распределения: {ex.Message}");
                MessageBox.Show($"Ошибка при обновлении распределения: {ex.Message}");
            }
        }

        private void Cancel(object parameter)
        {
            // Закрытие окна
            Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this)?.Close();
        }
    }
}
