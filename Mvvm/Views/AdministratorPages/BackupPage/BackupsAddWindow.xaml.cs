using System.Windows;
using TaskManagement.MVVM.ViewModels.AdministratorPageViewModel;

namespace TaskManagement.MVVM.Views.AdministratorPages.BackupsPage
{
    public partial class BackupsAddWindow : Window
    {
        public BackupsAddWindow(string currentUserLogin)
        {
            InitializeComponent();

            var viewModel = new BackupsAddViewModel(currentUserLogin);
            DataContext = viewModel;

            viewModel.SetDefaultValues();
        }
    }
}
