using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace TaskManagement.ViewModels.ManagerTask
{
    public class ManagerTaskViewModel
    {
        public ICommand ManageTasksCommand { get; set; }
        public ICommand LogoutCommand { get; set; }

        private AuthenticationService _authenticationService;

        public ManagerTaskViewModel(AuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
            ManageTasksCommand = new RelayCommand(ManageTasks);
            LogoutCommand = new RelayCommand(Logout);
        }

        private void ManageTasks()
        {
            MessageBox.Show("Управление задачами");
        }

        private void Logout()
        {
            var window = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.DataContext == this);
            window?.Close();
            Application.Current.Shutdown();
        }
    }
}
