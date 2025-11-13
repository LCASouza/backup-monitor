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
    public partial class JanelaConfigViewModel : ViewModelBase
    {
        [ObservableProperty]
        private JanelaConfigModel modelo = new JanelaConfigModel();

        public JanelaConfigViewModel()
        {
            var cfg = SessionContext.CurrentConfig!;

            Modelo.CadeiaConexaoAzure = cfg.AzureConnectionString;
            Modelo.ContainerAzure = cfg.AzureContainer;
            Modelo.Password = cfg.AccessPassword;

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
                AzureConnectionString = Modelo.CadeiaConexaoAzure,
                AzureContainer = Modelo.ContainerAzure
            };

            ConfigService.SaveConfig(config, Modelo.Password);

            SessionContext.CurrentConfig.AzureConnectionString = Modelo.CadeiaConexaoAzure;
            SessionContext.CurrentConfig.AzureContainer = Modelo.ContainerAzure;

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
