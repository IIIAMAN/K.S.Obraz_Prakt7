using System.Windows.Controls;
using TaskManagement.MVVM.ViewModels.AdministratorPageViewModel;

namespace TaskManagement.MVVM.Views.AdministratorPages
{
    public partial class MainPage : Page
    {
        public MainPage(string currentUserLogin)
        {
            InitializeComponent();
            DataContext = new MainPageViewModel(currentUserLogin);
        }
    }
}
