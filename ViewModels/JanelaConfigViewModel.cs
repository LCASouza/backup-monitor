using BackupMonitor.Models;
using BackupMonitor.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using BackupMonitor.Models;

namespace BackupMonitor.ViewModels
{
    public partial class JanelaConfigViewModel : ObservableObject
    {
        [ObservableProperty]
        private JanelaConfigModel modelo = new JanelaConfigModel();

        public JanelaConfigViewModel(string connectionString, string senhaAcesso)
        {
            Modelo.CadeiaConexaoAzure = connectionString;
            Modelo.Password = senhaAcesso;

            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                Modelo.SistemaOperacional = "Linux";
            }
            else
            {
                Modelo.SistemaOperacional = "Windows";
            }
        }

        [RelayCommand]
        private void GravarConfiguracao()
        {
            var config = new AppConfig
            {
                AzureConnectionString = Modelo.CadeiaConexaoAzure
            };

            ConfigService.SaveConfig(config, Modelo.Password);
            Modelo.StatusCompleto = "Configuração salva com sucesso!";
        }

        [RelayCommand]
        private void GravarSenha()
        {
            var config = new AppConfig
            {
                AccessPassword = Modelo.Password
            };
            
            ConfigService.SaveConfig(config, Modelo.Password);
            Modelo.StatusCompleto = "Senha salva com sucesso!";
        }

        [RelayCommand]
        private void ExcluirConfiguracao()
        {

        }

        [RelayCommand]
        private void TestarConexaoAzure()
        {
            
        }
    }
}
