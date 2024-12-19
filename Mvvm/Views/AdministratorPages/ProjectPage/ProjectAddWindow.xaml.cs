using System.Windows;
using TaskManagement.MVVM.ViewModels.AdministratorPageViewModel;

namespace TaskManagement.MVVM.Views.AdministratorPages.ProjectPage
{
    public partial class ProjectAddWindow : Window
    {
        public ProjectAddWindow(string currentUserLogin)
        {
            InitializeComponent();
            DataContext = new ProjectAddViewModel(currentUserLogin);
        }
    }
}
