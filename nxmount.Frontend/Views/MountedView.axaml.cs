using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using nxmount.Frontend.ViewModels;

namespace nxmount.Frontend.Views
{
    public partial class MountedView : UserControl
    {
        private MountedViewModel Model => (MountedViewModel)DataContext;
        public MountedView()
        {
            InitializeComponent();
        }


        private void OnUnmountClicked(object? sender, RoutedEventArgs e)
        {
            var window = Model.Parent;
            new Thread(() =>
            {
                window.MountService!.End();
                window.AppManager = null;
                window.MountService = null;

                Dispatcher.UIThread.Post(() =>
                {
                    window.TransitConfig();
                }, DispatcherPriority.Default);
            }).Start();
        }
    }
}
