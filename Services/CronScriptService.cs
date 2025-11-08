using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using BackupMonitor.Models;

namespace BackupMonitor.Services
{
    public static class CronScriptService
    {
        /// <summary>
        /// Garante que o diretório de backup exista e cria o script .sh com permissão de execução.
        /// Retorna o caminho do script criado.
        /// </summary>
        public static string EnsureScript(string homeDir, string scriptFileName, string scriptContent)
        {
            if (string.IsNullOrEmpty(homeDir))
                throw new ArgumentNullException(nameof(homeDir));

            var backupsDir = Path.Combine(homeDir, "backups");
            if (!Directory.Exists(backupsDir))
                Directory.CreateDirectory(backupsDir);

            var scriptsDir = Path.Combine(homeDir, ".backup_monitor");
            if (!Directory.Exists(scriptsDir))
                Directory.CreateDirectory(scriptsDir);

            var scriptPath = Path.Combine(scriptsDir, scriptFileName);

            // Escreve/replace do script
            File.WriteAllText(scriptPath, scriptContent, Encoding.UTF8);

            // Garante permissões executáveis (chmod +x)
            var psi = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-lc \"chmod +x '{scriptPath}'\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var p = Process.Start(psi);
            p?.WaitForExit();

            return scriptPath;
        }

        /// <summary>
        /// Gera o template do script de backup completo com pg_dump.
        /// Usa variáveis de ambiente para senha (PGPASSWORD) para evitar expor em comando.
        /// </summary>
        public static string GetCompleteBackupScriptTemplate(string pgHost, int pgPort, string pgUser, string dbName)
        {
            // usa $HOME e cria arquivo em $HOME/backups/
            return $@"#!/bin/bash
set -euo pipefail
HOME_DIR=""$HOME""
BACKUP_DIR=""$HOME_DIR/backups""
mkdir -p ""$BACKUP_DIR""

DATA=$(date +'%Y-%m-%d_%H-%M')
OUT=""$BACKUP_DIR/{dbName}_completo_$DATA.dump""

# Exemplo de uso: export PGPASSWORD='senha' antes ou use .pgpass
export PGPASSWORD=""${{PGPASSWORD:-}}""  # se PGPASSWORD não estiver setado, será vazio

pg_dump -h {pgHost} -p {pgPort} -U {pgUser} -F c {dbName} -f ""$OUT""
# opcional: compressar
# gzip -f ""$OUT""
echo ""Backup completo criado: $OUT""
";
        }

        /// <summary>
        /// Gera o template do script de backup incremental (ex.: tabela financeira ou parcial).
        /// </summary>
        public static string GetIncrementalBackupScriptTemplate(string pgHost, int pgPort, string pgUser, string dbName, string objectToDump)
        {
            // objectToDump pode ser '-t tabela' ou lista de tabelas
            return $@"#!/bin/bash
set -euo pipefail
HOME_DIR=""$HOME""
BACKUP_DIR=""$HOME_DIR/backups""
mkdir -p ""$BACKUP_DIR""

DATA=$(date +'%Y-%m-%d_%H-%M')
OUT=""$BACKUP_DIR/{dbName}_parcial_{objectToDump.Replace(" ", "_")}_$DATA.dump""

export PGPASSWORD=""${{PGPASSWORD:-}}"" 

pg_dump -h {pgHost} -p {pgPort} -U {pgUser} -F c {dbName} {objectToDump} -f ""$OUT""
echo ""Backup parcial criado: $OUT""
";
        }

        private static string GetBackupScriptTemplate(string tipo, AppConfig cfg)
        {
            string dbName = cfg.PostgresDbName ?? "financas";
            string script = $@"#!/bin/bash
set -euo pipefail
HOME_DIR=""$HOME""
BACKUP_DIR=""$HOME_DIR/backups""
mkdir -p ""$BACKUP_DIR""

DATA=$(date +'%Y-%m-%d_%H-%M')
OUT=""$BACKUP_DIR/{dbName}_{tipo}_$DATA.dump""

#Usa variável de ambiente PGPASSWORD
export PGPASSWORD=""{cfg.PostgresPassword}""

#Gera o dump
pg_dump -h {cfg.PostgresHost} -p {cfg.PostgresPort} -U {cfg.PostgresUser} -F c {dbName} -f ""$OUT""

#Compacta e criptografa o dump antes de enviar
gzip -f ""$OUT""
OUT_GZ=""$OUT.gz""

#Chama o app para fazer upload e registrar o hash no Azure
dotnet ""{AppContext.BaseDirectory}BackupMonitor.dll"" --auto-backup {tipo} --file ""$OUT_GZ""

echo ""✅ Backup {tipo} concluído com sucesso: $OUT_GZ""
";
            return script;
        }

        public static string CriarScript(string tipo, AppConfig cfg)
        {
            string home = Environment.GetEnvironmentVariable("HOME") ?? $"/home/{Environment.UserName}";
            string scriptsDir = Path.Combine(home, ".backup_monitor");
            Directory.CreateDirectory(scriptsDir);

            string scriptPath = Path.Combine(scriptsDir, $"backup_{tipo}.sh");
            string content = GetBackupScriptTemplate(tipo, cfg);
            File.WriteAllText(scriptPath, content, Encoding.UTF8);

            // Dar permissão de execução
            var psi = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"chmod +x '{scriptPath}'\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            Process.Start(psi)?.WaitForExit();

            return scriptPath;
        }
    }
}
