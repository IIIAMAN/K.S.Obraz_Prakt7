using System.Windows;
using System.Windows.Controls;
using TaskManagement.MVVM.ViewModels.AdministratorPageViewModel;

namespace TaskManagement.MVVM.Views.AdministratorPages
{
    public partial class StatusesPage : Page
    {
        public StatusesPage(string currentUserLogin)
        {
            InitializeComponent();

            var viewModel = new StatusesPageViewModel();
            viewModel.CurrentUserLogin = currentUserLogin;
            DataContext = viewModel;
        }
    }
}
