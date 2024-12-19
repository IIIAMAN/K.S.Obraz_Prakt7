using System.Windows;
using TaskManagement.MVVM.ViewModels.AdministratorPageViewModel;

namespace TaskManagement.MVVM.Views.AdministratorPages.RolePage
{
    public partial class RoleEditWindow : Window
    {
        public RoleEditWindow(int roleId, string currentUserLogin)
        {
            InitializeComponent();
            var viewModel = new RoleEditViewModel(roleId, currentUserLogin);
            DataContext = viewModel;
            viewModel.RoleUpdated += (s, e) => { DialogResult = true; Close(); };
        }
    }
}