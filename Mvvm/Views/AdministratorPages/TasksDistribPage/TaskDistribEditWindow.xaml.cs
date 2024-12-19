using System.Windows;
using TaskManagement.MVVM.ViewModels.AdministratorPageViewModel;

namespace TaskManagement.MVVM.Views.AdministratorPages.TasksDistribPage
{
    public partial class TaskDistribEditWindow : Window
    {
        public TaskDistribEditWindow(int distribId)
        {
            InitializeComponent();
            DataContext = new TaskDistribEditViewModel(distribId);
        }
    }
}
