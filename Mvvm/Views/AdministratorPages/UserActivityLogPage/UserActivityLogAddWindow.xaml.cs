using System.Windows;
using TaskManagement.MVVM.ViewModels.AdministratorPageViewModel;

namespace TaskManagement.MVVM.Views.AdministratorPages.UserActivityLogPage
{
    public partial class UserActivityLogAddWindow : Window
    {
        public UserActivityLogAddWindow(string currentUserLogin)
        {
            InitializeComponent();

            var viewModel = new UserActivityLogAddViewModel(currentUserLogin);
            DataContext = viewModel;

            viewModel.SetDefaultValues();
            viewModel.LoadComboBoxData();
        }
    }
}
