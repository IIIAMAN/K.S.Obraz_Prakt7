using System.Windows;
using TaskManagement.MVVM.ViewModels.AdministratorPageViewModel;

namespace TaskManagement.MVVM.Views.AdministratorPages.ProjectPage
{
    public partial class ProjectEditWindow : Window
    {
        public ProjectEditWindow(int projectId, string currentUserLogin)
        {
            InitializeComponent();

            var viewModel = new ProjectEditViewModel(projectId, currentUserLogin);
            DataContext = viewModel;

            viewModel.ProjectUpdated += ViewModel_ProjectUpdated;
        }

        private void ViewModel_ProjectUpdated(object sender, System.EventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
