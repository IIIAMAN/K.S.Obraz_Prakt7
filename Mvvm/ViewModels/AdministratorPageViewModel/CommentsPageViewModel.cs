using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TaskManagement.Models;
using TaskManagement.MVVM.Views.AdministratorPages.CommentPage;
using TaskManagement.TaskManagementDataSetTableAdapters;
using TaskManagement.ViewModels;

namespace TaskManagement.MVVM.ViewModels.AdministratorPageViewModel
{
    public class CommentsPageViewModel : ViewModelBase
    {
        private TaskManagementDataSet _dataSet;
        private CommentsTableAdapter _commentsTableAdapter;
        private UsersTableAdapter _usersTableAdapter;
        private TasksTableAdapter _tasksTableAdapter;
        private ObservableCollection<dynamic> _tasks;
        private dynamic _selectedComment;

        public ObservableCollection<dynamic> Tasks
        {
            get => _tasks;
            set
            {
                _tasks = value;
                OnPropertyChanged();
            }
        }

        public dynamic SelectedComment
        {
            get => _selectedComment;
            set
            {
                _selectedComment = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddCommentCommand { get; }
        public ICommand EditCommentCommand { get; }
        public ICommand DeleteCommentCommand { get; }

        public CommentsPageViewModel()
        {
            _dataSet = new TaskManagementDataSet();
            _commentsTableAdapter = new CommentsTableAdapter();
            _usersTableAdapter = new UsersTableAdapter();
            _tasksTableAdapter = new TasksTableAdapter();

            LoadTasks();

            AddCommentCommand = new RelayCommand(AddComment);
            EditCommentCommand = new RelayCommand(EditComment, CanEditOrDeleteComment);
            DeleteCommentCommand = new RelayCommand(DeleteComment, CanEditOrDeleteComment);
        }

        public void LoadTasks()
        {
            _tasksTableAdapter.Fill(_dataSet.Tasks);
            _usersTableAdapter.Fill(_dataSet.Users);
            _commentsTableAdapter.Fill(_dataSet.Comments);

            Tasks = new ObservableCollection<dynamic>(_dataSet.Tasks
                .Cast<TaskManagementDataSet.TasksRow>()
                .Select(task => new
                {
                    task.ID,
                    task.Title,
                    Comments = _dataSet.Comments
                        .Where(c => c.TaskID == task.ID)
                        .Select(comment => new
                        {
                            comment.ID,
                            comment.Title,
                            comment.CommentText,
                            User = _dataSet.Users.FirstOrDefault(u => u.ID == comment.UserID)?.Login,
                            Task = task.Title,
                            CreatedAt = comment.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                            UpdatedAt = comment.IsUpdatedAtNull() ? (string)null : comment.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                        }).ToList()
                }).ToList());
        }

        private void AddComment(object parameter)
        {
            var addCommentWindow = new CommentAddWindow();
            var viewModel = (CommentAddViewModel)addCommentWindow.DataContext;

            viewModel.CommentAdded += (s, args) => LoadTasks();

            addCommentWindow.ShowDialog();
        }

        private void EditComment(object parameter)
        {
            if (SelectedComment != null)
            {
                var editWindow = new CommentEditWindow(SelectedComment.ID);
                var viewModel = (CommentEditViewModel)editWindow.DataContext;

                viewModel.CommentUpdated += (s, args) => LoadTasks();

                if (editWindow.ShowDialog() == true)
                {
                    LoadTasks();
                }
            }
        }

        private void DeleteComment(object parameter)
        {
            if (SelectedComment != null)
            {
                if (MessageBox.Show("Вы уверены, что хотите удалить этот комментарий?", "Удаление комментария", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        var commentRow = _dataSet.Comments.FindByID(SelectedComment.ID);
                        if (commentRow != null)
                        {
                            commentRow.Delete();
                            _commentsTableAdapter.Update(_dataSet.Comments);
                            LoadTasks();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении комментария: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private bool CanEditOrDeleteComment(object parameter)
        {
            return SelectedComment != null;
        }
    }
}
