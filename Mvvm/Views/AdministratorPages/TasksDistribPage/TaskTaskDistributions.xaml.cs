using System.Windows.Controls;
using TaskManagement.MVVM.ViewModels.AdministratorPageViewModel;

namespace TaskManagement.MVVM.Views.AdministratorPages.TasksDistribPage
{
    public partial class TaskDistributionsPage : Page
    {
        public TaskDistributionsPage()
        {
            InitializeComponent();
            DataContext = new TaskDistributionsPageViewModel();
        }

        private void OnCardSelected(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext is TaskDistributionsPageViewModel viewModel)
            {
                var border = sender as Border;
                var selectedTaskDistribution = border?.DataContext;
                viewModel.SelectedTaskDistribution = selectedTaskDistribution;
            }
        }
    }
}
