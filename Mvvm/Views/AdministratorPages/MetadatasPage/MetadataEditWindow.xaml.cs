using System.Windows;
using TaskManagement.MVVM.ViewModels.AdministratorPageViewModel;

namespace TaskManagement.MVVM.Views.AdministratorPages.MetadataPage
{
    public partial class MetadataEditWindow : Window
    {
        public MetadataEditWindow(int metadataId, string currentUserLogin)
        {
            InitializeComponent();

            var viewModel = new MetadataEditViewModel(metadataId, currentUserLogin);
            DataContext = viewModel;

            viewModel.MetadataUpdated += ViewModel_MetadataUpdated;
        }

        private void ViewModel_MetadataUpdated(object sender, System.EventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
