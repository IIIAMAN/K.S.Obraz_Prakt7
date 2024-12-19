﻿using System.Windows.Input;
using TaskManagement.ViewModels;
using System.Windows.Controls;
using TaskManagement.MVVM.Views.AdministratorPages.BackupPage;
using TaskManagement.MVVM.Views.AdministratorPages.TaskHistoryPage;
using TaskManagement.MVVM.Views.AdministratorPages.TaskPage;
using TaskManagement.MVVM.Views.AdministratorPages;

namespace TaskManagement.MVVM.ViewModels.ChiefTaskExecutor
{
    public class MainChiefTaskExecutorViewModel : ViewModelBase
    {
        private string _currentUserLogin;
        public string UserLogin => _currentUserLogin;

        public ICommand LoadPageCommand { get; }
        public ICommand LogoutCommand { get; }

        private Frame _mainFrame;
        public Frame MainFrame
        {
            get => _mainFrame;
            set
            {
                _mainFrame = value;
                OnPropertyChanged();
            }
        }

        public MainChiefTaskExecutorViewModel(string currentUserLogin)
        {
            _currentUserLogin = currentUserLogin;
            LoadPageCommand = new RelayCommand(ExecuteLoadPage);
            LogoutCommand = new RelayCommand(ExecuteLogout);
        }

        private void ExecuteLoadPage(object parameter)
        {
            if (parameter == null) return;

            string pageName = parameter.ToString();
            Page page = null;

            switch (pageName)
            {
                case "MainPage":
                    page = new MainPage(_currentUserLogin);
                    break;
                case "TasksPage":
                    page = new TasksPage(_currentUserLogin);
                    break;
                case "TaskHistoryPage":
                    page = new TaskHistoryPage(_currentUserLogin);
                    break;
                case "UserActivityLogPage":
                    
                    break;
                case "BackupsPage":
                    page = new BackupsPage(_currentUserLogin);
                    break;
                default:
                    break;
            }

            if (page != null)
            {
                MainFrame.Content = page;
            }
        }

        private void ExecuteLogout(object parameter)
        {
    
        }
    }
}
