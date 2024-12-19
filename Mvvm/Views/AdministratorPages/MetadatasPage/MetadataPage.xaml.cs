using System.Windows;
using System.Windows.Controls;
using TaskManagement.MVVM.ViewModels.AdministratorPageViewModel;

namespace TaskManagement.MVVM.Views.AdministratorPages.MetadatasPage
{
    public partial class MetadataPage : Page
    {
        public MetadataPage(string currentUserLogin)
        {
            InitializeComponent();
            DataContext = new MetadataPageViewModel(currentUserLogin);
        }
    }
}
