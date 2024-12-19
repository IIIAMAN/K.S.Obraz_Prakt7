using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using TaskManagement.ViewModels;
using TaskManagement.Views;

namespace TaskManagement
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var loginWindow = new LoginWindow();
            loginWindow.Show();
        }
    }
}
