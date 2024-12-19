using System.Windows;
using TaskManagement.MVVM.ViewModels.AdministratorPageViewModel;

namespace TaskManagement.MVVM.Views.AdministratorPages.RolePage
{
    public partial class RoleAddWindow : Window
    {
        public RoleAddWindow(string currentUserLogin)
        {
            InitializeComponent();
            var viewModel = new RoleAddViewModel(currentUserLogin);
            DataContext = viewModel;
        }
    }
}