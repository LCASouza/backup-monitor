using Avalonia.Controls;
using BackupMonitor.ViewModels;

namespace BackupMonitor.Views;

public partial class JanelaRestore : Window
{
    public JanelaRestore()
    {
        InitializeComponent();
        DataContext = new JanelaRestoreViewModel();
    }
}