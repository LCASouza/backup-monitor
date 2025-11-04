using BackupMonitor.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackupMonitor.Models;

namespace BackupMonitor.ViewModels
{
    public partial class JanelaBackupViewModel : ObservableObject
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
                // string scriptPath = "/home/lucas/backup_completo.sh";

                // CronService.ScheduleBackup(
                //     scriptPath,
                //     DataInicialCompleto ?? DateTimeOffset.Now,
                //     HoraBackupCompleto ?? TimeSpan.FromHours(2),
                //     FrequenciaSelecionadaCompleto,
                //     "backup_completo"
                // );

                WindowsTaskSchedulerService.ScheduleBackup(
                    exePath: "C:\\Caminho\\Para\\SeuBackup.exe",
                    startDate: Modelo.DataInicialCompleto.Value.DateTime,
                    hora: Modelo.HoraBackupCompleto.Value,
                    frequencia: Modelo.FrequenciaSelecionadaCompleto,
                    nomeTarefa: "Backup_Completo"
                );

                Modelo.StatusCompleto = $"✅ Backup COMPLETO agendado para {Modelo.DataInicialCompleto:d} às {Modelo.HoraBackupCompleto:hh\\:mm} ({Modelo.FrequenciaSelecionadaCompleto})";
            }
            catch (Exception ex)
            {
                Modelo.StatusCompleto = $"❌ Erro ao agendar backup: {ex.Message}";
                return;
            }
        }

        [RelayCommand]
        private void CancelarAgendamentoCompleto()
        {
            WindowsTaskSchedulerService.RemoveBackupJob("Backup_Completo");

            Modelo.StatusCompleto = "Agendamento cancelado";
        }


        [RelayCommand]
        private void AgendarBackupIncremental()
        {
            try
            {
                // string scriptPath = "/home/lucas/backup_completo.sh";

                // CronService.ScheduleBackup(
                //     scriptPath,
                //     DataInicialCompleto ?? DateTimeOffset.Now,
                //     HoraBackupCompleto ?? TimeSpan.FromHours(2),
                //     FrequenciaSelecionadaCompleto,
                //     "backup_completo"
                // );

                WindowsTaskSchedulerService.ScheduleBackup(
                    exePath: "C:\\Caminho\\Para\\SeuBackup.exe",
                    startDate: Modelo.DataInicialIncremental.Value.DateTime,
                    hora: Modelo.HoraBackupIncremental.Value,
                    frequencia: Modelo.FrequenciaSelecionadaIncremental,
                    nomeTarefa: "Backup_Incremental"
                );

                Modelo.StatusCompleto = $"✅ Backup COMPLETO agendado para {Modelo.DataInicialCompleto:d} às {Modelo.HoraBackupCompleto:hh\\:mm} ({Modelo.FrequenciaSelecionadaCompleto})";
            }
            catch (Exception ex)
            {
                Modelo.StatusCompleto = $"❌ Erro ao agendar backup: {ex.Message}";
                return;
            }
        }


        [RelayCommand]
        private void CancelarAgendamentoIncremental()
        {
            Modelo.StatusIncremental = "Agendamento cancelado";
        }
    }
}
