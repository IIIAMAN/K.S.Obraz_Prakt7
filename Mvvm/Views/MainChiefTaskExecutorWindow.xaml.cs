using System.Windows;
using TaskManagement.MVVM.ViewModels;
using TaskManagement.MVVM.ViewModels.ChiefTaskExecutor;

namespace TaskManagement.MVVM.Views
{
    public partial class MainChiefTaskExecutorWindow : Window
    {
        public MainChiefTaskExecutorWindow(string currentUserLogin)
        {
            InitializeComponent();
            DataContext = new MainChiefTaskExecutorViewModel(currentUserLogin);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void pnlControlBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void pnlControlBar_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.Cursor = System.Windows.Input.Cursors.Arrow;
        }
    }
}
