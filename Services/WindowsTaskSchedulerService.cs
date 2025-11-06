using Microsoft.Win32.TaskScheduler;
using System;
using System.Linq;

namespace BackupMonitor.Services
{
    public class WindowsTaskSchedulerService
    {
        public static void ScheduleBackup(string exePath, DateTime startDate, TimeSpan hora, string frequencia, string nomeTarefa)
        {
            using (TaskService ts = new TaskService())
            {
                TaskDefinition td = ts.NewTask();
                td.RegistrationInfo.Description = "Backup automático do banco de dados para Azure";

                //Define o gatilho conforme a frequência
                DateTime startBoundary = startDate.Date + hora;
                if (frequencia == "Diário")
                    td.Triggers.Add(new DailyTrigger { StartBoundary = startBoundary });
                else if (frequencia == "Semanal")
                    td.Triggers.Add(new WeeklyTrigger { StartBoundary = startBoundary });
                else if (frequencia == "Mensal")
                    td.Triggers.Add(new MonthlyTrigger { StartBoundary = startBoundary });

                //Define a ação (executa o .exe ou script)
                td.Actions.Add(new ExecAction(exePath, null, null));

                //Registra a tarefa
                ts.RootFolder.RegisterTaskDefinition(
                    nomeTarefa,
                    td,
                    TaskCreation.CreateOrUpdate,
                    userId: Environment.UserName,
                    password: null,
                    logonType: TaskLogonType.InteractiveToken
                );
            }
        }

        public static void RemoveBackupJob(string nomeTarefa)
        {
            using var ts = new TaskService();
            if (ts.RootFolder.GetTasks().Any(t => t.Name == nomeTarefa))
                ts.RootFolder.DeleteTask(nomeTarefa, false);
        }
    }
}
