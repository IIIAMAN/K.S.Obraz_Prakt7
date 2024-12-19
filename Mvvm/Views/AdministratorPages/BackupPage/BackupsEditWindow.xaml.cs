using System.Windows;
using TaskManagement.MVVM.ViewModels.AdministratorPageViewModel;

namespace TaskManagement.MVVM.Views.AdministratorPages.BackupsPage
{
    public partial class BackupsEditWindow : Window
    {
        public BackupsEditWindow(int backupId, string currentUserLogin)
        {
            InitializeComponent();

            var viewModel = new BackupsEditViewModel(backupId, currentUserLogin);
            DataContext = viewModel;

            viewModel.BackupUpdated += ViewModel_BackupUpdated;
        }

        private void ViewModel_BackupUpdated(object sender, System.EventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
