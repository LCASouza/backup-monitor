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
                string jobName = "Backup_Completo";

                if (Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    string home = "/home/" + Environment.UserName;
                    string scriptCompleto = @"
                    #!/bin/bash
                    DATA=$(date +'%Y-%m-%d_%H-%M')
                    pg_dump -h localhost -U backupuser -F c financas > ""/home/$USER/backups/backup_completo_$DATA.dump""
                    ";
                    string scriptCompletoPath = $"{home}/backup_completo.sh";

                    // Criar script se não existir
                    if (!File.Exists(scriptCompletoPath))
                    {
                        CronService.CriarScriptBackup(scriptCompletoPath, scriptCompleto);
                    }

                    // Agendar no cron
                    CronService.ScheduleBackup(
                        scriptCompletoPath,
                        Modelo.DataInicialCompleto!.Value,
                        Modelo.HoraBackupCompleto!.Value,
                        Modelo.FrequenciaSelecionadaCompleto!,
                        "Backup_Completo"
                    );
                }
                else
                {
                    // WINDOWS
                    WindowsTaskSchedulerService.ScheduleBackup(
                        exePath: "C:\\Caminho\\Para\\SeuBackup.exe",
                        startDate: Modelo.DataInicialCompleto.Value.DateTime,
                        hora: Modelo.HoraBackupCompleto.Value,
                        frequencia: Modelo.FrequenciaSelecionadaCompleto,
                        nomeTarefa: jobName
                    );
                }

                Modelo.StatusCompleto =
                    $"✅ Backup COMPLETO agendado para {Modelo.DataInicialCompleto:d} às {Modelo.HoraBackupCompleto:hh\\:mm} ({Modelo.FrequenciaSelecionadaCompleto})";
            }
            catch (Exception ex)
            {
                Modelo.StatusCompleto = $"❌ Erro ao agendar backup: {ex.Message}";
            }
        }


        [RelayCommand]
        private void CancelarAgendamentoCompleto()
        {
            string jobName = "Backup_Completo";

            if (Environment.OSVersion.Platform == PlatformID.Unix)
                CronService.RemoveBackupJob(jobName);
            else
                WindowsTaskSchedulerService.RemoveBackupJob(jobName);

            Modelo.StatusCompleto = "❌ Agendamento completo cancelado";
        }

        [RelayCommand]
        private void AgendarBackupIncremental()
        {
            try
            {
                string jobName = "Backup_Incremental";

                if (Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    string scriptPath = "/home/lucas/backup_incremental.sh";

                    CronService.ScheduleBackup(
                        scriptPath,
                        Modelo.DataInicialIncremental ?? DateTimeOffset.Now,
                        Modelo.HoraBackupIncremental ?? TimeSpan.FromHours(2),
                        Modelo.FrequenciaSelecionadaIncremental,
                        jobName
                    );
                }
                else
                {
                    WindowsTaskSchedulerService.ScheduleBackup(
                        exePath: "C:\\Caminho\\Para\\SeuBackupIncremental.exe",
                        startDate: Modelo.DataInicialIncremental.Value.DateTime,
                        hora: Modelo.HoraBackupIncremental.Value,
                        frequencia: Modelo.FrequenciaSelecionadaIncremental,
                        nomeTarefa: jobName
                    );
                }

                Modelo.StatusIncremental =
                    $"✅ Backup INCREMENTAL agendado para {Modelo.DataInicialIncremental:d} às {Modelo.HoraBackupIncremental:hh\\:mm} ({Modelo.FrequenciaSelecionadaIncremental})";
            }
            catch (Exception ex)
            {
                Modelo.StatusIncremental = $"❌ Erro ao agendar backup: {ex.Message}";
            }
        }

        [RelayCommand]
        private void CancelarAgendamentoIncremental()
        {
            Modelo.StatusIncremental = "Agendamento cancelado";

            string jobName = "Backup_Incremental";

            if (Environment.OSVersion.Platform == PlatformID.Unix)
                CronService.RemoveBackupJob(jobName);
            else
                WindowsTaskSchedulerService.RemoveBackupJob(jobName);

            Modelo.StatusIncremental = "Agendamento incremental cancelado";
        }
    }
}
