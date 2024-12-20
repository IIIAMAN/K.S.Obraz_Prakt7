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
    public class CommentAddViewModel : ViewModelBase
    {
        private TaskManagementDataSet _dataSet;
        private CommentsTableAdapter _commentsTableAdapter;
        private TasksTableAdapter _tasksTableAdapter;
        private UsersTableAdapter _usersTableAdapter;

        private string _title;
        private string _commentText;
        private int _selectedTaskId;
        private int _selectedUserId;
        private DateTime _creationDate;
        private DateTime _updatedAt;

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        public string CommentText
        {
            get => _commentText;
            set
            {
                _commentText = value;
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

        public DateTime CreationDate
        {
            get => _creationDate;
            set
            {
                _creationDate = value;
                OnPropertyChanged();
            }
        }

        public DateTime UpdatedAt
        {
            get => _updatedAt;
            set
            {
                _updatedAt = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<TaskManagementDataSet.TasksRow> Tasks { get; set; }
        public ObservableCollection<TaskManagementDataSet.UsersRow> Users { get; set; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler CommentAdded;

        public CommentAddViewModel()
        {
            _dataSet = new TaskManagementDataSet();
            _commentsTableAdapter = new CommentsTableAdapter();
            _tasksTableAdapter = new TasksTableAdapter();
            _usersTableAdapter = new UsersTableAdapter();

            LoadComboBoxData();
            SetDefaultValues();

            SaveCommand = new RelayCommand(SaveComment);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void LoadComboBoxData()
        {
            _tasksTableAdapter.Fill(_dataSet.Tasks);
            _usersTableAdapter.Fill(_dataSet.Users);

            Tasks = new ObservableCollection<TaskManagementDataSet.TasksRow>(_dataSet.Tasks.Cast<TaskManagementDataSet.TasksRow>());
            Users = new ObservableCollection<TaskManagementDataSet.UsersRow>(_dataSet.Users.Cast<TaskManagementDataSet.UsersRow>());
        }

        private void SetDefaultValues()
        {
            CreationDate = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }

        private void SaveComment(object parameter)
        {
            try
            {
                if (string.IsNullOrEmpty(Title) || string.IsNullOrEmpty(CommentText) || SelectedTaskId == 0 || SelectedUserId == 0)
                {
                    MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Console.WriteLine($"Adding new comment: Title = {Title}, CommentText = {CommentText}, TaskID = {SelectedTaskId}, UserID = {SelectedUserId}, CreationDate = {CreationDate}, UpdatedAt = {UpdatedAt}");

                _commentsTableAdapter.Insert(Title, CommentText, CreationDate, SelectedTaskId, SelectedUserId, CreationDate, UpdatedAt);

                MessageBox.Show("Комментарий успешно добавлен!");

                CommentAdded?.Invoke(this, EventArgs.Empty);

                Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this)?.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении комментария: {ex.Message}");
                MessageBox.Show($"Ошибка при добавлении комментария: {ex.Message}");
            }
        }

        private void Cancel(object parameter)
        {
            Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this)?.Close();
        }
    }
}
