using System.Windows.Controls;
using TaskManagement.MVVM.ViewModels.AdministratorPageViewModel;

namespace TaskManagement.MVVM.Views.AdministratorPages.CommentPage
{
    public partial class CommentsPage : Page
    {
        public CommentsPage()
        {
            InitializeComponent();
            DataContext = new CommentsPageViewModel();
        }

        private void OnCardSelected(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext is CommentsPageViewModel viewModel)
            {
                var border = sender as Border;
                var selectedComment = border?.DataContext;
                viewModel.SelectedComment = selectedComment;
            }
        }
    }
}
