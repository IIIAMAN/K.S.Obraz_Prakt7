using System.Windows.Controls;
using TaskManagement.MVVM.ViewModels.AdministratorPageViewModel;

namespace TaskManagement.MVVM.Views.AdministratorPages.PrioritiesPage
{
    public partial class PrioritiesPage : Page
    {
        public PrioritiesPage(string currentUserLogin)
        {
            InitializeComponent();

            // Передаем значение в DataContext
            if (DataContext is PrioritiesPageViewModel viewModel)
            {
                viewModel.CurrentUserLogin = currentUserLogin;
            }
        }
    }
}
