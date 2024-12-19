using System.Windows;
using TaskManagement.MVVM.ViewModels.AdministratorPageViewModel;

namespace TaskManagement.MVVM.Views.AdministratorPages.CommentPage
{
    public partial class CommentAddWindow : Window
    {
        public CommentAddWindow()
        {
            InitializeComponent();
            DataContext = new CommentAddViewModel();
        }
    }
}
