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
}