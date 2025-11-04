using System.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BackupMonitor.Models;

public partial class JanelaBackupModel : ObservableObject
{
    [ObservableProperty]
    private DateTimeOffset? dataInicialCompleto = DateTimeOffset.Now;

    [ObservableProperty]
    private DateTimeOffset? dataInicialIncremental = DateTimeOffset.Now;

    [ObservableProperty]
    private TimeSpan? horaBackupCompleto = DateTime.Now.TimeOfDay;

    [ObservableProperty]
    private TimeSpan? horaBackupIncremental = DateTime.Now.TimeOfDay;

    [ObservableProperty]
    private List<string> frequenciasDisponiveis = new List<string> { "Diário", "Semanal", "Mensal" };

    [ObservableProperty]
    private string frequenciaSelecionadaCompleto = "Diário";

    [ObservableProperty]
    private string frequenciaSelecionadaIncremental = "Diário";

    [ObservableProperty]
    private string statusCompleto = "Pronto";
        
    [ObservableProperty]
    private string statusIncremental = "Pronto";
}