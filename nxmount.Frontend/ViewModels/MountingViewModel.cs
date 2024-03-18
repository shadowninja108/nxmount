using CommunityToolkit.Mvvm.ComponentModel;

namespace nxmount.Frontend.ViewModels
{
    public partial class MountingViewModel : ViewModelBase
    {
        [ObservableProperty]
        private MainWindowViewModel _parent;

        public MountingViewModel(MainWindowViewModel parent)
        {
            Parent = parent;
        }
    }
}
