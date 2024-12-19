using System.Windows;
using System.Windows.Controls;
using TaskManagement.MVVM.ViewModels.AdministratorPageViewModel;

namespace TaskManagement.MVVM.Views.AdministratorPages.UserActivityLogPage
{
    public partial class UserActivityLogsPage : Page
    {
        public UserActivityLogsPage(string currentUserLogin)
        {
            InitializeComponent();
            DataContext = new UserActivityLogsPageViewModel(currentUserLogin);
        }
    }
}
