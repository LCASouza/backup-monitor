using System.Text.Json;
using System.IO;
using BackupMonitor.Models;
using System;
using System.Diagnostics;

namespace BackupMonitor.Services;

public class ConfigService
{
    private static readonly string configPath = Path.Combine(AppContext.BaseDirectory, "config.enc");
    private static readonly CriptografiaService cryptoService = new();

    public static void SaveConfig(AppConfig newConfig, string password)
    {
        AppConfig mergedConfig;

        // Se já existir o arquivo, carregar e mesclar
        if (File.Exists(configPath))
        {
            try
            {
                var existingConfig = LoadConfig(password);
                mergedConfig = MergeConfigs(existingConfig, newConfig);
            }
            catch
            {
                // Caso a senha não bata ou o arquivo esteja corrompido,
                // salva apenas o novo conteúdo
                mergedConfig = newConfig;
            }
        }
        else
        {
            mergedConfig = newConfig;
        }

        // Serializa e criptografa novamente
        string json = JsonSerializer.Serialize(mergedConfig, new JsonSerializerOptions { WriteIndented = true });
        string tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, json);

        cryptoService.EncryptFile(tempFile, configPath, password);
        File.Delete(tempFile);
    }

    public static AppConfig LoadConfig(string password)
    {
        if (!File.Exists(configPath))
            throw new FileNotFoundException("Arquivo de configuração não encontrado.");

        string tempFile = Path.GetTempFileName();
        cryptoService.DecryptFile(configPath, tempFile, password);

        string json = File.ReadAllText(tempFile);
        File.Delete(tempFile);

        return JsonSerializer.Deserialize<AppConfig>(json);
    }

    private static AppConfig MergeConfigs(AppConfig existing, AppConfig updated)
    {
        // Copia apenas os campos não nulos / não vazios
        if (!string.IsNullOrEmpty(updated.AzureConnectionString))
            existing.AzureConnectionString = updated.AzureConnectionString;

        if (!string.IsNullOrEmpty(updated.AccessPassword))
            existing.AccessPassword = updated.AccessPassword;

        if (!string.IsNullOrEmpty(updated.PostgresHost))
            existing.PostgresHost = updated.PostgresHost;

        if (updated.PostgresPort > 0)
            existing.PostgresPort = updated.PostgresPort;

        if (!string.IsNullOrEmpty(updated.PostgresUser))
            existing.PostgresUser = updated.PostgresUser;

        if (!string.IsNullOrEmpty(updated.PostgresPassword))
            existing.PostgresPassword = updated.PostgresPassword;

        if (!string.IsNullOrEmpty(updated.PostgresDbName))
            existing.PostgresDbName = updated.PostgresDbName;

        if (updated.BackupCompletoDataInicial.HasValue)
            existing.BackupCompletoDataInicial = updated.BackupCompletoDataInicial;
        if (updated.BackupCompletoHora.HasValue)
            existing.BackupCompletoHora = updated.BackupCompletoHora;
        if (!string.IsNullOrEmpty(updated.BackupCompletoFrequencia))
            existing.BackupCompletoFrequencia = updated.BackupCompletoFrequencia;

        if (updated.BackupIncrementalDataInicial.HasValue)
            existing.BackupIncrementalDataInicial = updated.BackupIncrementalDataInicial;
        if (updated.BackupIncrementalHora.HasValue)
            existing.BackupIncrementalHora = updated.BackupIncrementalHora;
        if (!string.IsNullOrEmpty(updated.BackupIncrementalFrequencia))
            existing.BackupIncrementalFrequencia = updated.BackupIncrementalFrequencia;

        if (!string.IsNullOrEmpty(updated.CaminhoExeBackupWindows))
            existing.CaminhoExeBackupWindows = updated.CaminhoExeBackupWindows;

        return existing;
    }
}
