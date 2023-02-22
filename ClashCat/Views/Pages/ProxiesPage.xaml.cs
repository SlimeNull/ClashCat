using Wpf.Ui.Common.Interfaces;

namespace ClashCat.Views.Pages
{
    /// <summary>
    /// Interaction logic for DataView.xaml
    /// </summary>
    public partial class ProxiesPage : INavigableView<ViewModels.ProxiesViewModel>
    {
        public ViewModels.ProxiesViewModel ViewModel
        {
            get;
        }

        public ProxiesPage(ViewModels.ProxiesViewModel viewModel)
        {
            ViewModel = viewModel;

            InitializeComponent();
        }
    }
}
