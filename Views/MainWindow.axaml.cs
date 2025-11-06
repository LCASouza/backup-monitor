using Avalonia.Controls;
using BackupMonitor.ViewModels;

namespace BackupMonitor.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
        
        this.Opened += MainWindow_Opened;
    }

    private async void MainWindow_Opened(object? sender, System.EventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await vm.InitializeAsync(this);

            if (!vm.isOk)
                Close();
        }
    }
}