using Avalonia.Controls;
using BackupMonitor.ViewModels;

namespace BackupMonitor.Views;

public partial class JanelaBackup : Window
{
    public JanelaBackup(string accessPassword)
    {
        InitializeComponent();
        DataContext = new JanelaBackupViewModel(accessPassword);
    }
}