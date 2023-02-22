using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wpf.Ui.Common.Interfaces;

namespace ClashCat.ViewModels
{
    public partial class GeneralViewModel : ObservableObject, INavigationAware
    {
        [ObservableProperty]
        private int _counter = 0;

        [ObservableProperty]
        private bool _popupShow = false;

        public void OnNavigatedTo()
        {
        }

        public void OnNavigatedFrom()
        {
        }

        [RelayCommand]
        private void OnCounterIncrement()
        {
            Counter++;
        }
    }
}
