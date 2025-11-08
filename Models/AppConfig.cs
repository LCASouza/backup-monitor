using System;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackupMonitor.Models;

public class AppConfig
{
    public string AzureConnectionString { get; set; }
    public string PostgresHost { get; set; }
    public int PostgresPort { get; set; }
    public string PostgresUser { get; set; }
    public string PostgresPassword { get; set; }
    public string PostgresDbName { get; set; }
    public string PostgresConnectionString { get; set; }
    public string AccessPassword { get; set; }

    //Agendamento Completo
    public DateTime? BackupCompletoDataInicial { get; set; }
    public TimeSpan? BackupCompletoHora { get; set; }
    public string BackupCompletoFrequencia { get; set; }

    //Agendamento Incremental
    public DateTime? BackupIncrementalDataInicial { get; set; }
    public TimeSpan? BackupIncrementalHora { get; set; }
    public string BackupIncrementalFrequencia { get; set; }


    public string CaminhoExeBackupWindows { get; set; }
}