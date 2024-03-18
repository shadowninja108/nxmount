using CommunityToolkit.Mvvm.ComponentModel;
using nxmount.Apps;
using nxmount.Frontend.Model;

namespace nxmount.Frontend.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private Config _config;

        [ObservableProperty]
        private ViewModelBase _contentViewModel;

        public ConfigViewModel ConfigView { get; }
        public MountingViewModel MountingView { get; }
        public MountedViewModel MountedViewModel { get; }
        public PreferencesViewModel PreferencesViewModel { get; }

        public AppManager? AppManager;
        public IMountService? MountService;
        [ObservableProperty]
        private string _mountPoint;

        public MainWindowViewModel()
        {
            Config = new Config();
            ConfigView = new ConfigViewModel(this);
            MountingView = new MountingViewModel(this);
            MountedViewModel = new MountedViewModel(this);
            PreferencesViewModel = new PreferencesViewModel(this);

            ContentViewModel = ConfigView;
        }

        public void TransitConfig()
        {
            ContentViewModel = ConfigView;
        }

        public void TransitMounting()
        {
            ContentViewModel = MountingView;
        }

        public void TransitMounted()
        {
            ContentViewModel = MountedViewModel;
        }

        public void TransitPreferences()
        {
            ContentViewModel = PreferencesViewModel;
        }
    }
}
