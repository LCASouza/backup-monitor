using System.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BackupMonitor.Models;

public partial class MainWindowModel : ObservableObject
{
    [ObservableProperty]
    private string dbHost = "localhost";
    
    [ObservableProperty]
    private int dbPort = 5432;
    
    [ObservableProperty]
    private string dbUser = "usuario";
    
    [ObservableProperty]
    private string dbPassword = string.Empty;
    
    [ObservableProperty] 
    private string dbName = "testes";

    [ObservableProperty]
    private string status = "Pronto";
}