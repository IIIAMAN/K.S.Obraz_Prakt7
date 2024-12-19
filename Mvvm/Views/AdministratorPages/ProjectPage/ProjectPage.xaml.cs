using System.Windows.Controls;
using TaskManagement.MVVM.ViewModels.AdministratorPageViewModel;
using TaskManagement.ViewModels.AdministratorPageViewModels;

namespace TaskManagement.MVVM.Views.AdministratorPages.ProjectPage
{
    public partial class ProjectPage : Page
    {
        public ProjectPage(string currentUserLogin)
        {
            InitializeComponent();

            var viewModel = new ProjectsPageViewModel();
            DataContext = viewModel;

            if (DataContext is ProjectsPageViewModel vm)
            {
                vm.CurrentUserLogin = currentUserLogin;
            }
        }

        private void OnCardSelected(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext is ProjectsPageViewModel viewModel)
            {
                var border = sender as Border;
                var selectedProject = border?.DataContext;
                viewModel.SelectedProject = selectedProject;
            }
        }
    }
}
