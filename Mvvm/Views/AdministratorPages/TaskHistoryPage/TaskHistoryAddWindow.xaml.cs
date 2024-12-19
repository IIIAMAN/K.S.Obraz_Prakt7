using System;
using System.Windows;
using TaskManagement.MVVM.ViewModels.AdministratorPageViewModel;

namespace TaskManagement.MVVM.Views.AdministratorPages.TaskHistoryPage
{
    public partial class TaskHistoryAddWindow : Window
    {
        public TaskHistoryAddWindow(string currentUserLogin)
        {
            InitializeComponent();

            var viewModel = new TaskHistoryAddViewModel(currentUserLogin);
            DataContext = viewModel;

            viewModel.SetDefaultValues();
            viewModel.LoadComboBoxData();
        }
    }
}
