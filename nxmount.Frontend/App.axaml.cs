using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using nxmount.Frontend.ViewModels;
using nxmount.Frontend.Views;

namespace nxmount.Frontend;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel()
            };
        }
        // else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        // {
        //     singleViewPlatform.MainView = new MainView
        //     {
        //         DataContext = new ConfigViewModel()
        //     };
        // }

        base.OnFrameworkInitializationCompleted();
    }
}
