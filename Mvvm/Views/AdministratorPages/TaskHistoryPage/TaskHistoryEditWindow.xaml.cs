using System.Windows;
using TaskManagement.MVVM.ViewModels.AdministratorPageViewModel;

namespace TaskManagement.MVVM.Views.AdministratorPages.TaskHistoryPage
{
    public partial class TaskHistoryEditWindow : Window
    {
        public TaskHistoryEditWindow(int historyId, string currentUserLogin)
        {
            InitializeComponent();

            var viewModel = new TaskHistoryEditViewModel(historyId, currentUserLogin);
            DataContext = viewModel;

            viewModel.HistoryUpdated += ViewModel_HistoryUpdated;
        }

        private void ViewModel_HistoryUpdated(object sender, System.EventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
