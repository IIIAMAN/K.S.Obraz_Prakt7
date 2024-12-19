using System.Windows;
using System.Windows.Controls;
using TaskManagement.MVVM.ViewModels.AdministratorPageViewModel;

namespace TaskManagement.MVVM.Views.AdministratorPages.BackupPage
{
    public partial class BackupsPage : Page
    {
        public BackupsPage(string currentUserLogin)
        {
            InitializeComponent();
            DataContext = new BackupsPageViewModel(currentUserLogin);
        }
    }
}
