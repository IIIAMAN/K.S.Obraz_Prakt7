using System.Windows;
using TaskManagement.MVVM.ViewModels.AdministratorPageViewModel;

namespace TaskManagement.MVVM.Views.AdministratorPages.PrioritiesPage
{
    public partial class PriorityAddWindow : Window
    {
        public PriorityAddWindow(string currentUserLogin)
        {
            InitializeComponent();

            var viewModel = new PriorityAddViewModel();
            DataContext = viewModel;

            if (DataContext is PriorityAddViewModel addViewModel)
            {
                addViewModel.CurrentUserLogin = currentUserLogin;
            }
        }
    }
}
