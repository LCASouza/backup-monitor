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

//Teste Git
namespace BackupMonitor.ViewModels
{
    public partial class JanelaBackupViewModel : ViewModelBase
    {
        [ObservableProperty]
        private JanelaBackupModel modelo = new JanelaBackupModel();

        public JanelaBackupViewModel()
        {
            
        }

        [RelayCommand]
        private void AgendarBackupCompleto()
        {
            try
            {
                var cfg = SessionContext.CurrentConfig!;

                //Criar o script completo
                string scriptPath = CronScriptService.CriarScript("completo", cfg);

                //Agendar no cron
                CronService.ScheduleBackup(
                    scriptPath,
                    Modelo.DataInicialCompleto ?? DateTimeOffset.Now,
                    Modelo.HoraBackupCompleto ?? TimeSpan.FromHours(2),
                    Modelo.FrequenciaSelecionadaCompleto ?? "Diário",
                    "Backup_Completo"
                );

                Modelo.StatusCompleto = $"✅ Backup COMPLETO agendado com sucesso ({Modelo.FrequenciaSelecionadaCompleto})";
            }
            catch (Exception ex)
            {
                Modelo.StatusCompleto = $"❌ Erro ao agendar backup completo: {ex.Message}";
            }
        }

        [RelayCommand]
        private void CancelarAgendamentoCompleto()
        {
            CronService.RemoveBackupJob("Backup_Completo");
            Modelo.StatusCompleto = "⛔ Agendamento completo cancelado.";
        }

        [RelayCommand]
        private void AgendarBackupIncremental()
        {
            try
            {
                var cfg = SessionContext.CurrentConfig!;

                string scriptPath = CronScriptService.CriarScript("incremental", cfg);

                CronService.ScheduleBackup(
                    scriptPath,
                    Modelo.DataInicialIncremental ?? DateTimeOffset.Now,
                    Modelo.HoraBackupIncremental ?? TimeSpan.FromHours(2),
                    Modelo.FrequenciaSelecionadaIncremental ?? "Diário",
                    "Backup_Incremental"
                );

                Modelo.StatusIncremental = $"✅ Backup INCREMENTAL agendado com sucesso ({Modelo.FrequenciaSelecionadaIncremental})";
            }
            catch (Exception ex)
            {
                Modelo.StatusIncremental = $"❌ Erro ao agendar backup incremental: {ex.Message}";
            }
        }

        [RelayCommand]
        private void CancelarAgendamentoIncremental()
        {
            CronService.RemoveBackupJob("Backup_Incremental");
            Modelo.StatusIncremental = "⛔ Agendamento incremental cancelado.";
        }
    }
}
