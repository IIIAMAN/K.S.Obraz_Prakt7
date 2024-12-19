using System.Windows;
using TaskManagement.MVVM.ViewModels.AdministratorPageViewModel;

namespace TaskManagement.MVVM.Views.AdministratorPages.StatusPage
{
    public partial class StatusEditWindow : Window
    {
        public StatusEditWindow(int statusId, string currentUserLogin)
        {
            InitializeComponent();

            var viewModel = new StatusEditViewModel(statusId, currentUserLogin);
            DataContext = viewModel;

            // Подписываемся на событие StatusUpdated для закрытия окна после успешного обновления статуса
            viewModel.StatusUpdated += ViewModel_StatusUpdated;
        }

        private void ViewModel_StatusUpdated(object sender, System.EventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is StatusEditViewModel viewModel)
            {
                viewModel.SaveCommand.Execute(null);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
