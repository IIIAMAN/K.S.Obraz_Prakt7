using System.Windows;
using TaskManagement.MVVM.ViewModels.AdministratorPageViewModel;

namespace TaskManagement.MVVM.Views.AdministratorPages.UserActivityLogPage
{
    public partial class UserActivityLogEditWindow : Window
    {
        public UserActivityLogEditWindow(int logId, string currentUserLogin)
        {
            InitializeComponent();

            var viewModel = new UserActivityLogEditViewModel(logId, currentUserLogin);
            DataContext = viewModel;

            viewModel.LogUpdated += ViewModel_LogUpdated;
        }

        private void ViewModel_LogUpdated(object sender, System.EventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
