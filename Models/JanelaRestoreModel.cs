using System.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BackupMonitor.Models;

public partial class JanelaRestoreModel : ObservableObject
{
    [ObservableProperty]
    private List<string> origens = new() { "Local", "Azure" };

    [ObservableProperty]
    private string origemSelecionada = "Local";

    [ObservableProperty]
    private string caminhoArquivo = string.Empty;

    [ObservableProperty]
    private string accessPassword = string.Empty;

    [ObservableProperty]
    private string bancoDestino = "bd_restore";

    [ObservableProperty]
    private string postgresHost = "localhost";

    [ObservableProperty]
    private string postgresPort = "5432";

    [ObservableProperty]
    private string postgresUserRestore = "user_restore";

    [ObservableProperty]
    private string postgresPassword = string.Empty;

    [ObservableProperty]
    private string status = "Aguardando seleção de arquivo...";

    [ObservableProperty]
    private ObservableCollection<string> backupsAzure = new();

    [ObservableProperty]
    private string backupSelecionadoAzure = string.Empty;
}