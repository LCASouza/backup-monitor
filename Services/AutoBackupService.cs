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
        private readonly AzureBlobService _azureService = new();

        public void ExecutarBackupAutomatico(AppConfig cfg, string tipo, string filePath)
        {
            try
            {
                Console.WriteLine($"📦 Iniciando backup automático ({tipo})...");
                Console.WriteLine($"Destino: {filePath}");

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

                Console.WriteLine("🔐 Calculando hash...");
                string hash = _hashService.ComputeSha256(dumpPath);

                // ✅ Corrigido: gerar o .enc no mesmo diretório, sem Path.Combine com arquivo
                string encPath = Path.ChangeExtension(dumpPath, ".enc");

                Console.WriteLine("🔒 Criptografando backup...");
                _cryptoService.EncryptFile(dumpPath, encPath, cfg.AccessPassword);

                string blobName = $"{cfg.PostgresDbName}_{tipo}_{DateTime.Now:yyyyMMdd_HHmmss}_{hash[..12]}.enc";
                Console.WriteLine($"☁️  Enviando para o Azure como '{blobName}'...");

                _azureService.Upload(encPath, blobName);

                // Limpeza
                if (File.Exists(dumpPath)) File.Delete(dumpPath);
                if (File.Exists(encPath)) File.Delete(encPath);

                Console.WriteLine($"✅ Backup automático do banco '{cfg.PostgresDbName}' enviado com sucesso!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Falha no backup automático: {ex.Message}");
            }
        }
    }
}
