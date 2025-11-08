using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace BackupMonitor.Services
{
    public static class CronService
    {
        private static string GetCronTimeString(DateTimeOffset date, TimeSpan time, string frequency)
        {
            int minute = time.Minutes;
            int hour = time.Hours;
            string dayOfMonth = "*";
            string month = "*";
            string dayOfWeek = "*";

            switch (frequency)
            {
                case "Diário":
                    break;
                case "Semanal":
                    // cron espera números 0-6 (Dom-Sab). Transformamos DayOfWeek em número.
                    int dow = (int)date.DayOfWeek; // Sunday=0
                    dayOfWeek = dow.ToString();
                    break;
                case "Mensal":
                    dayOfMonth = date.Day.ToString();
                    break;
                default:
                    break;
            }

            return $"{minute} {hour} {dayOfMonth} {month} {dayOfWeek}";
        }

        public static void ScheduleBackup(string scriptPath, DateTimeOffset date, TimeSpan time, string frequency, string jobName)
        {
            if (string.IsNullOrEmpty(scriptPath) || !File.Exists(scriptPath))
                throw new FileNotFoundException("Script não encontrado", scriptPath);

            string cronTime = GetCronTimeString(date, time, frequency);
            // comando executado por cron: chamar /bin/bash -lc "script" para carregar env do shell
            string cronCommand = $"{cronTime} /bin/bash -lc \"'{scriptPath}'\" # {jobName}";

            string currentCrontab = "";
            try
            {
                currentCrontab = RunCommand("crontab -l");
            }
            catch
            {
                currentCrontab = string.Empty;
            }

            var filtered = new StringBuilder();
            using (var reader = new StringReader(currentCrontab))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!line.Contains($"# {jobName}"))
                        filtered.AppendLine(line);
                }
            }

            filtered.AppendLine(cronCommand);

            RunCommand("crontab -", filtered.ToString());
        }

        public static void RemoveBackupJob(string jobName)
        {
            string currentCrontab = "";
            try
            {
                currentCrontab = RunCommand("crontab -l");
            }
            catch
            {
                return;
            }

            var filtered = new StringBuilder();
            using (var reader = new StringReader(currentCrontab))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!line.Contains($"# {jobName}"))
                        filtered.AppendLine(line);
                }
            }

            RunCommand("crontab -", filtered.ToString());
        }

        private static string RunCommand(string cmd, string input = "")
        {
            var psi = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = "-c \"" + cmd + "\"",
                RedirectStandardOutput = true,
                RedirectStandardInput = !string.IsNullOrEmpty(input),
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = psi };
            process.Start();

            if (!string.IsNullOrEmpty(input))
            {
                process.StandardInput.Write(input);
                process.StandardInput.Close();
            }

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (process.ExitCode != 0 && !cmd.Contains("crontab -l"))
                throw new Exception($"Erro ao executar '{cmd}': {error}");

            return output;
        }
    }
}
