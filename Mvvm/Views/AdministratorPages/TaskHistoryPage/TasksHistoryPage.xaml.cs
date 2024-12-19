using System.Windows.Controls;
using TaskManagement.MVVM.ViewModels.AdministratorPageViewModel;

namespace TaskManagement.MVVM.Views.AdministratorPages.TaskHistoryPage
{
    public partial class TaskHistoryPage : Page
    {
        public TaskHistoryPage(string currentUserLogin)
        {
            InitializeComponent();
            DataContext = new TaskHistoryPageViewModel(currentUserLogin);
        }

        private void OnCardSelected(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext is TaskHistoryPageViewModel viewModel)
            {
                var border = sender as Border;
                var selectedTaskHistory = border?.DataContext as TaskManagementDataSet.TaskHistoryRow;
                if (selectedTaskHistory != null)
                {
                    viewModel.SelectedTaskHistory = selectedTaskHistory;
                }
            }
        }
    }
}
