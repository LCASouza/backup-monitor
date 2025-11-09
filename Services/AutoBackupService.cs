using System;
using System.IO;
using BackupMonitor.Models;
using BackupMonitor.Services;

namespace BackupMonitor.Services
{
    public class AutoBackupService
    {
        private readonly PostgresService _pgService = new();
        private readonly HashService _hashService = new();
        private readonly CriptografiaService _cryptoService = new();

        public void ExecutarBackupAutomatico(AppConfig cfg, string tipo, string filePath)
        {
            string logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".backup_monitor", "logs");
            Directory.CreateDirectory(logDir);
            string logFile = Path.Combine(logDir, $"auto_backup_{DateTime.Now:yyyyMMdd_HHmmss}.log");

            void Log(string msg)
            {
                File.AppendAllText(logFile, $"[{DateTime.Now:HH:mm:ss}] {msg}\n");
            }
                
            try
            {
                Log($"📦 Iniciando backup automático ({tipo})...");
                Log($"Destino: {filePath}");

                // Garante que o diretório existe
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                // Executa o pg_dump e grava diretamente no caminho solicitado
                string dumpPath = _pgService.BackupDatabase(
                    cfg.PostgresHost,
                    cfg.PostgresPort,
                    cfg.PostgresDbName,
                    cfg.PostgresUser,
                    cfg.PostgresPassword,
                    filePath
                );

                Log("🔐 Calculando hash...");
                string hash = _hashService.ComputeSha256(dumpPath);

                // ✅ Corrigido: gerar o .enc no mesmo diretório, sem Path.Combine com arquivo
                string encPath = Path.ChangeExtension(dumpPath, ".enc");

                Log("🔒 Criptografando backup...");
                _cryptoService.EncryptFile(dumpPath, encPath, cfg.AccessPassword);

                string blobName = $"{cfg.PostgresDbName}_{tipo}_{DateTime.Now:yyyyMMdd_HHmmss}_{hash[..12]}.enc";
                Log($"☁️  Enviando para o Azure como '{blobName}'...");

                var azureService = new AzureBlobService(cfg);
                azureService.Upload(encPath, blobName);

                // Limpeza
                if (File.Exists(dumpPath)) File.Delete(dumpPath);
                if (File.Exists(encPath)) File.Delete(encPath);

                Log($"✅ Backup automático do banco '{cfg.PostgresDbName}' enviado com sucesso!");
            }
            catch (Exception ex)
            {
                Log($"❌ Falha no backup automático: {ex.Message}");
            }
        }
    }
}
