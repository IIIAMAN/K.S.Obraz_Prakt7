using System.Windows;
using TaskManagement.MVVM.ViewModels.AdministratorPageViewModel;

namespace TaskManagement.MVVM.Views.AdministratorPages.StatusPage
{
    public partial class StatusAddWindow : Window
    {
        public StatusAddWindow(string currentUserLogin)
        {
            InitializeComponent();
            DataContext = new StatusAddViewModel(currentUserLogin);
        }
    }
}
