using System;
using System.Windows;
using TaskManagement.MVVM.ViewModels.AdministratorPageViewModel;

namespace TaskManagement.MVVM.Views.AdministratorPages.UserPage
{
    public partial class UserEditWindow : Window
    {
        public event EventHandler UserUpdated; // Событие для обновления данных после редактирования

        public UserEditWindow(TaskManagementDataSet.UsersRow userRow = null, string currentUserLogin = null)
        {
            InitializeComponent();

            // Создаем и назначаем DataContext
            var viewModel = new UserEditViewModel();
            DataContext = viewModel;

            // Инициализируем ViewModel с userId и currentUserLogin
            viewModel.Initialize(userRow?.ID ?? -1, currentUserLogin);

            // Подписываемся на событие UserUpdated
            viewModel.UserUpdated += ViewModel_UserUpdated;
        }

        private void ViewModel_UserUpdated(object sender, EventArgs e)
        {
            UserUpdated?.Invoke(this, EventArgs.Empty);
            this.DialogResult = true;
            this.Close();
        }
    }
}
