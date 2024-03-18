using Avalonia.Controls;
using nxmount.Frontend.ViewModels;

namespace nxmount.Frontend.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel Model => (MainWindowViewModel)DataContext;
    public MainWindow()
    {
        InitializeComponent();
    }
}
