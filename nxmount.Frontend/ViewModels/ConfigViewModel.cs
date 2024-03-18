using CommunityToolkit.Mvvm.ComponentModel;

namespace nxmount.Frontend.ViewModels;

public partial class ConfigViewModel : ViewModelBase
{
    [ObservableProperty]
    private MainWindowViewModel _parent;

    public ConfigViewModel(MainWindowViewModel parent)
    {
        Parent = parent;
    }
}
