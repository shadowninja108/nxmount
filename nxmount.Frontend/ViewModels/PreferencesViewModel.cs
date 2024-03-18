using CommunityToolkit.Mvvm.ComponentModel;

namespace nxmount.Frontend.ViewModels
{
    public partial class PreferencesViewModel : ViewModelBase
    {
        [ObservableProperty] 
        private MainWindowViewModel _parent;

        public PreferencesViewModel(MainWindowViewModel parent)
        {
            Parent = parent;
        }
    }
}
