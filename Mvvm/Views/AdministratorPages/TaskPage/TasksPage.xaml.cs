using System.Windows.Controls;
using TaskManagement.MVVM.ViewModels.AdministratorPageViewModel;

namespace TaskManagement.MVVM.Views.AdministratorPages.TaskPage
{
    public partial class TasksPage : Page
    {
        public TasksPage(string currentUserLogin)
        {
            InitializeComponent();

            if (DataContext is TasksPageViewModel viewModel)
            {
                viewModel.CurrentUserLogin = currentUserLogin;
            }
        }

        private void OnCardSelected(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext is TasksPageViewModel viewModel)
            {
                var border = sender as Border;
                var selectedTask = border?.DataContext;
                viewModel.SelectedTask = selectedTask;
            }
        }
    }
}
