using BackupMonitor.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BackupMonitor.Views;
using BackupMonitor.Models;
using System.Threading.Tasks;
using System;
using System.Diagnostics;
using System.IO;

namespace BackupMonitor.ViewModels
{
    public partial class JanelaSenhaViewModel : ViewModelBase
    {
        private readonly JanelaSenha janela;

        [ObservableProperty]
        private string senhaAcesso;

        [ObservableProperty]
        private string status = "";

        private bool primeiroAcesso = false;

        public JanelaSenhaViewModel(bool firstRun)
        {
            primeiroAcesso = firstRun;
            if (primeiroAcesso)
            {
                Status = "Primeiro acesso detectado. Defina uma senha de acesso.";
            }
        }

        public JanelaSenhaViewModel(JanelaSenha window)
        {
            janela = window;
        }

        [RelayCommand]
        private void ConfirmarSenha()
        {
            try
            {
                if (primeiroAcesso)
                {
                    var config = new AppConfig
                    {
                        AccessPassword = SenhaAcesso
                    };

                    ConfigService.SaveConfig(config, SenhaAcesso);
                    Status = "Senha salva com sucesso! Reiniciando a aplicação para acessar as configurações.";

                    Task.Delay(2000).Wait();

                    ReiniciarAplicacao();
                }
                else
                {
                    var cfg = ConfigService.LoadConfig(SenhaAcesso);

                    janela.Confirmar(SenhaAcesso);

                    Status = "Senha confirmada com sucesso.";
                    janela.Close();
                }
            }
            catch
            {
                Status = "Senha incorreta. Tente novamente.";
            }
        }

        [RelayCommand]
        private void Cancelar()
        {
            janela.Cancelar();
        }

        private void ReiniciarAplicacao()
        {
            try
            {
                string appPath;

                // 🔹 Caso esteja rodando via "dotnet run" (projeto .dll)
                if (AppContext.BaseDirectory.EndsWith("Debug/net9.0/") || AppContext.BaseDirectory.Contains("net"))
                {
                    string dllPath = Path.Combine(AppContext.BaseDirectory, "BackupMonitor.dll");
                    appPath = "dotnet";

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = appPath,
                        Arguments = $"\"{dllPath}\"",
                        UseShellExecute = false
                    });
                }
                else
                {
                    // 🔹 Caso esteja compilado como .exe (Windows) ou binário
                    appPath = Environment.ProcessPath!;
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = appPath,
                        UseShellExecute = false
                    });
                }

                // 🔻 Encerra a instância atual
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Status = $"Erro ao reiniciar aplicação: {ex.Message}";
            }
        }

    }
}
