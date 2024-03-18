using CommunityToolkit.Mvvm.ComponentModel;

namespace nxmount.Frontend.ViewModels
{
    public partial class MountedViewModel : ViewModelBase
    {
        [ObservableProperty]
        private MainWindowViewModel _parent;

        public string MountMessage => $"Mounted at {Parent.MountPoint}";

        public MountedViewModel(MainWindowViewModel parent)
        {
            Parent = parent;
        }
    }
}
