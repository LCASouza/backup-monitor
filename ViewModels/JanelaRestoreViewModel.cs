using BackupMonitor.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackupMonitor.Models;
using System.IO;
using Avalonia.Controls;

namespace BackupMonitor.ViewModels
{
    public partial class JanelaRestoreViewModel : ViewModelBase
    {
        [ObservableProperty]
        private JanelaRestoreModel modelo = new JanelaRestoreModel();

        public JanelaRestoreViewModel()
        {
            Modelo.AccessPassword = SessionContext.AccessPassword!;;
        }

        [RelayCommand]
        private async Task SelecionarArquivoAsync(Window owner)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Selecionar backup",
                AllowMultiple = false,
                Filters =
                {
                    new FileDialogFilter { Name = "Arquivos de backup", Extensions = { "enc", "dump" } }
                }
            };

            var files = await dialog.ShowAsync(owner);
            if (files?.Length > 0)
                Modelo.CaminhoArquivo = files[0];
        }

        [RelayCommand]
        private async Task RestaurarBackupAsync()
        {
            try
            {
                if (Modelo.AccessPassword == null)
                {
                    Modelo.Status = "⚠️ Forneça a senha de acesso para realizar o backup.";
                    return;
                }

                if (Modelo.AccessPassword != SessionContext.AccessPassword)
                {
                    Modelo.Status = "❌ Senha de acesso incorreta.";
                    return;
                }

                if (string.IsNullOrWhiteSpace(Modelo.PostgresPassword))
                {
                    Modelo.Status = "⚠️ Forneça a senha do banco de dados PostgreSQL.";
                    return;
                }

                Modelo.Status = "⏳ Iniciando processo de restauração...";

                //Se for .enc → descriptografar para .dump
                string arquivoFinal = Modelo.CaminhoArquivo;

                if (arquivoFinal.EndsWith(".enc"))
                {
                    var tempDump = Path.ChangeExtension(Modelo.CaminhoArquivo, ".dump");
                    new CriptografiaService().DecryptFile(Modelo.CaminhoArquivo, tempDump, Modelo.AccessPassword);
                    arquivoFinal = tempDump;
                }

                //Executar o pg_restore
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "pg_restore",
                    Arguments = $"-h {Modelo.PostgresHost} -p {Modelo.PostgresPort} -U {Modelo.PostgresUserRestore} -d {Modelo.BancoDestino} -c \"{arquivoFinal}\"",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                psi.Environment["PGPASSWORD"] = Modelo.PostgresPassword;

                var proc = System.Diagnostics.Process.Start(psi)!;
                string output = await proc.StandardOutput.ReadToEndAsync();
                string error = await proc.StandardError.ReadToEndAsync();
                await proc.WaitForExitAsync();

                if (proc.ExitCode == 0)
                    Modelo.Status = $"✅ Restauração concluída com sucesso no banco {Modelo.BancoDestino}";
                else
                    Modelo.Status = $"❌ Falha ao restaurar backup:\n{error}";
            }
            catch (Exception ex)
            {
                Modelo.Status = $"❌ Erro inesperado: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task RestaurarBackupAzureAsync()
        {
            try
            {
                if (Modelo.AccessPassword == null)
                {
                    Modelo.Status = "⚠️ Forneça a senha de acesso para realizar o backup.";
                    return;
                }

                if (Modelo.AccessPassword != SessionContext.AccessPassword)
                {
                    Modelo.Status = "❌ Senha de acesso incorreta.";
                    return;
                }

                if (string.IsNullOrWhiteSpace(Modelo.PostgresPassword))
                {
                    Modelo.Status = "⚠️ Forneça a senha do banco de dados PostgreSQL.";
                    return;
                }

                if (string.IsNullOrWhiteSpace(Modelo.BackupSelecionadoAzure))
                {
                    Modelo.Status = "⚠️ Selecione um backup da nuvem primeiro.";
                    return;
                }

                Modelo.Status = "☁️ Baixando backup do Azure...";
                var cfg = SessionContext.CurrentConfig!;
                var azure = new AzureBlobService(cfg);

                string tmpDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".backup_monitor", "tmp");
                Directory.CreateDirectory(tmpDir);

                string localPath = Path.Combine(tmpDir, Path.GetFileName(Modelo.BackupSelecionadoAzure));
                await azure.DownloadAsync(Modelo.BackupSelecionadoAzure, localPath);

                Modelo.Status = "🔒 Descriptografando arquivo...";
                string arquivoFinal = localPath;
                if (localPath.EndsWith(".enc"))
                {
                    string dumpPath = Path.ChangeExtension(localPath, ".dump");
                    new CriptografiaService().DecryptFile(localPath, dumpPath, Modelo.AccessPassword);
                    arquivoFinal = dumpPath;
                }

                Modelo.Status = "🧩 Restaurando backup para o banco...";
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "pg_restore",
                    Arguments = $"-h {Modelo.PostgresHost} -p {Modelo.PostgresPort} -U postgres -d {Modelo.BancoDestino} -c \"{arquivoFinal}\"",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                psi.Environment["PGPASSWORD"] = Modelo.PostgresPassword;

                var proc = System.Diagnostics.Process.Start(psi)!;
                string output = await proc.StandardOutput.ReadToEndAsync();
                string error = await proc.StandardError.ReadToEndAsync();
                await proc.WaitForExitAsync();

                if (proc.ExitCode == 0)
                    Modelo.Status = $"✅ Restauração do Azure concluída com sucesso no banco {Modelo.BancoDestino}";
                else
                    Modelo.Status = $"❌ Falha ao restaurar backup:\n{error}";
            }
            catch (Exception ex)
            {
                Modelo.Status = $"❌ Erro inesperado: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task ListarBackupsAzureAsync()
        {
            try
            {
                Modelo.Status = "☁️ Conectando ao Azure Blob Storage...";

                var cfg = SessionContext.CurrentConfig!;
                var azure = new AzureBlobService(cfg);
                var blobs = await azure.ListarBlobsAsync();

                Modelo.BackupsAzure.Clear();
                foreach (var blob in blobs)
                    Modelo.BackupsAzure.Add(blob);

                Modelo.Status = $"✅ {Modelo.BackupsAzure.Count} backups encontrados no Azure.";
            }
            catch (Exception ex)
            {
                Modelo.Status = $"❌ Falha ao listar backups: {ex.Message}";
            }
        }
    }
}