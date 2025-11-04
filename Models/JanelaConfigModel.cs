using System.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BackupMonitor.Models;

public partial class JanelaConfigModel : ObservableObject
{
    [ObservableProperty]
    private List<string> sistemaOperacionais = new List<string> { "Windows", "Linux" };

    [ObservableProperty]
    private string sistemaOperacional = string.Empty;

    [ObservableProperty]
    private string cadeiaConexaoAzure;

    [ObservableProperty]
    private string cadeiaConexaoPostgres;

    [ObservableProperty]
    private string statusCompleto = "Pronto";

    [ObservableProperty]
    private string password = "";
}