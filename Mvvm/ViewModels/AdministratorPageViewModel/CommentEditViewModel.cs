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
    public class CommentEditViewModel : ViewModelBase
    {
        private TaskManagementDataSet _dataSet;
        private CommentsTableAdapter _commentsTableAdapter;
        private TasksTableAdapter _tasksTableAdapter;
        private UsersTableAdapter _usersTableAdapter;

        private int _commentId;
        private string _title;
        private string _commentText;
        private int _selectedTaskId;
        private int _selectedUserId;
        private DateTime _creationDate;
        private DateTime _updatedAt;

        public int CommentId
        {
            get => _commentId;
            private set
            {
                _commentId = value;
                OnPropertyChanged();
            }
        }

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

        public event EventHandler CommentUpdated;

        public CommentEditViewModel(int commentId)
        {
            _dataSet = new TaskManagementDataSet();
            _commentsTableAdapter = new CommentsTableAdapter();
            _tasksTableAdapter = new TasksTableAdapter();
            _usersTableAdapter = new UsersTableAdapter();

            CommentId = commentId;

            LoadCommentData(commentId);
            LoadComboBoxData();

            SaveCommand = new RelayCommand(SaveComment);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void LoadCommentData(int commentId)
        {
            _commentsTableAdapter.Fill(_dataSet.Comments);
            var commentRow = _dataSet.Comments.FindByID(commentId);

            if (commentRow != null)
            {
                Title = commentRow.Title;
                CommentText = commentRow.CommentText;
                SelectedTaskId = commentRow.TaskID;
                SelectedUserId = commentRow.UserID;
                CreationDate = commentRow.CreationDate;
                UpdatedAt = DateTime.Now;
            }
        }

        private void LoadComboBoxData()
        {
            _tasksTableAdapter.Fill(_dataSet.Tasks);
            _usersTableAdapter.Fill(_dataSet.Users);

            Tasks = new ObservableCollection<TaskManagementDataSet.TasksRow>(_dataSet.Tasks.Cast<TaskManagementDataSet.TasksRow>());
            Users = new ObservableCollection<TaskManagementDataSet.UsersRow>(_dataSet.Users.Cast<TaskManagementDataSet.UsersRow>());
        }

        private void SaveComment(object parameter)
        {
            try
            {
                // Проверка на заполнение всех полей
                if (string.IsNullOrEmpty(Title) || string.IsNullOrEmpty(CommentText) || SelectedTaskId == 0 || SelectedUserId == 0)
                {
                    MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Логирование данных перед обновлением
                Console.WriteLine($"Updating comment: ID = {CommentId}, Title = {Title}, CommentText = {CommentText}, TaskID = {SelectedTaskId}, UserID = {SelectedUserId}, UpdatedAt = {UpdatedAt}");

                // Обновление существующего комментария
                var commentRow = _dataSet.Comments.FindByID(CommentId);

                if (commentRow != null)
                {
                    commentRow.Title = Title;
                    commentRow.CommentText = CommentText;
                    commentRow.TaskID = SelectedTaskId;
                    commentRow.UserID = SelectedUserId;
                    commentRow.UpdatedAt = UpdatedAt;

                    _commentsTableAdapter.Update(commentRow);

                    MessageBox.Show("Комментарий успешно обновлен!");

                    CommentUpdated?.Invoke(this, EventArgs.Empty);

                    // Закрытие окна
                    Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this)?.Close();
                }
                else
                {
                    MessageBox.Show("Комментарий не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                Console.WriteLine($"Ошибка при обновлении комментария: {ex.Message}");
                MessageBox.Show($"Ошибка при обновлении комментария: {ex.Message}");
            }
        }

        private void Cancel(object parameter)
        {
            // Закрытие окна
            Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this)?.Close();
        }
    }
}
