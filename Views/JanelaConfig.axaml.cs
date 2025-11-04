using Avalonia.Controls;
using BackupMonitor.ViewModels;

namespace BackupMonitor.Views;

public partial class JanelaConfig : Window
{
    public JanelaConfig(string connectionString = "", string senhaAcesso = "")
    {
        InitializeComponent();
        DataContext = new JanelaConfigViewModel(connectionString, senhaAcesso);
    }
}