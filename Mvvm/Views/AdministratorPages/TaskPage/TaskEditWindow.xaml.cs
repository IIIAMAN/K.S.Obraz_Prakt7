using System.Windows;
using TaskManagement.MVVM.ViewModels.AdministratorPageViewModel;

namespace TaskManagement.MVVM.Views.AdministratorPages.TasksPage
{
    public partial class TaskEditWindow : Window
    {
        public TaskEditWindow(int taskId)
        {
            InitializeComponent();

            var viewModel = new TaskEditViewModel(taskId);
            DataContext = viewModel;

            // Подписываемся на событие TaskUpdated для закрытия окна после успешного обновления задачи
            viewModel.TaskUpdated += ViewModel_TaskUpdated;
        }

        private void ViewModel_TaskUpdated(object sender, System.EventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
