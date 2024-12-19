using System.Windows;
using TaskManagement.MVVM.ViewModels.AdministratorPageViewModel;

namespace TaskManagement.MVVM.Views.AdministratorPages.TasksDistribPage
{
    public partial class TaskDistribAddWindow : Window
    {
        public TaskDistribAddWindow()
        {
            InitializeComponent();
            DataContext = new TaskDistribAddViewModel();
        }
    }
}
