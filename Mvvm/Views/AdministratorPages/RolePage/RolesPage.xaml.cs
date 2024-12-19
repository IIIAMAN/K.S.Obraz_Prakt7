using System.Windows.Controls;
using TaskManagement.MVVM.ViewModels.AdministratorPageViewModel;

namespace TaskManagement.MVVM.Views.AdministratorPages.RolePage
{
    public partial class RolesPage : Page
    {
        public RolesPage(string currentUserLogin)
        {
            InitializeComponent();
            DataContext = new RolesPageViewModel();
        }
    }
}
