using System.Windows;
using TaskManagement.MVVM.ViewModels.AdministratorPageViewModel;

namespace TaskManagement.MVVM.Views.AdministratorPages.MetadataPage
{
    public partial class MetadataAddWindow : Window
    {
        public MetadataAddWindow(string currentUserLogin)
        {
            InitializeComponent();

            var viewModel = new MetadataAddViewModel(currentUserLogin);
            DataContext = viewModel;
        }
    }
}
