using System.Windows;
using TaskManagement.MVVM.ViewModels.AdministratorPageViewModel;

namespace TaskManagement.MVVM.Views.AdministratorPages.CommentPage
{
    public partial class CommentEditWindow : Window
    {
        public CommentEditWindow(int commentId)
        {
            InitializeComponent();

            var viewModel = new CommentEditViewModel(commentId);
            DataContext = viewModel;

            // Подписываемся на событие CommentUpdated для закрытия окна после успешного обновления комментария
            viewModel.CommentUpdated += ViewModel_CommentUpdated;
        }

        private void ViewModel_CommentUpdated(object sender, System.EventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
