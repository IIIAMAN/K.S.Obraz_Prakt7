using System.Windows.Controls;
using System.Windows.Input;
using TaskManagement.ViewModels.AdministratorPageViewModels;

namespace TaskManagement.MVVM.Views.AdministratorPages
{
    public partial class UsersPage : Page
    {
        public UsersPage(string currentUserLogin)
        {
            InitializeComponent();

            if (DataContext is UsersPageViewModel viewModel)
            {
                viewModel.CurrentUserLogin = currentUserLogin;
            }
        }

        private void OnCardSelected(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is UsersPageViewModel viewModel)
            {
                var border = sender as Border;
                var selectedUser = border?.DataContext;
                viewModel.SelectedUser = selectedUser;
            }
        }
    }
}
