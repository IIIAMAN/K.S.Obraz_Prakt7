using System;
using System.Windows;
using TaskManagement.ViewModels.AdministratorPageViewModels;

namespace TaskManagement.MVVM.Views.AdministratorPages.UserPage
{
    public partial class UserAddWindow : Window
    {
        public UserAddWindow(string currentUserLogin)
        {
            InitializeComponent();

            var viewModel = new UserAddViewModel(currentUserLogin);
            DataContext = viewModel;

            viewModel.UserAdded += ViewModel_UserAdded;
        }

        private void ViewModel_UserAdded(object sender, EventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
