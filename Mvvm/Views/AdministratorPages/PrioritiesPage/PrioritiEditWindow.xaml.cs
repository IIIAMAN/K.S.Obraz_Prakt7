using System.Windows;
using TaskManagement.MVVM.ViewModels.AdministratorPageViewModel;

namespace TaskManagement.MVVM.Views.AdministratorPages.PrioritiesPage
{
    public partial class PriorityEditWindow : Window
    {
        public PriorityEditWindow(int priorityId, string currentUserLogin)
        {
            InitializeComponent();

            var viewModel = new PriorityEditViewModel(priorityId, currentUserLogin);
            DataContext = viewModel;

            // Подписываемся на событие PriorityUpdated для закрытия окна после успешного обновления приоритета
            viewModel.PriorityUpdated += ViewModel_PriorityUpdated;
        }

        private void ViewModel_PriorityUpdated(object sender, System.EventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
