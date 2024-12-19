using System.Windows;
using TaskManagement.MVVM.ViewModels.AdministratorPageViewModel;

namespace TaskManagement.MVVM.Views.AdministratorPages.TasksPage
{
    public partial class TaskAddWindow : Window
    {
        public TaskAddWindow(string currentUserLogin)
        {
            InitializeComponent();
            DataContext = new TaskAddViewModel(currentUserLogin);
        }
    }


}
