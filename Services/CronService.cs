using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace BackupMonitor.Services
{
    public class CronService
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
                    dayOfWeek = date.DayOfWeek.ToString().Substring(0, 3).ToLower();
                    break;
                case "Mensal":
                    dayOfMonth = date.Day.ToString();
                    break;
            }

            return $"{minute} {hour} {dayOfMonth} {month} {dayOfWeek}";
        }

        public static void ScheduleBackup(string scriptPath, DateTimeOffset date, TimeSpan time, string frequency, string jobName)
        {
            string cronTime = GetCronTimeString(date, time, frequency);
            string cronCommand = $"{cronTime} /bin/bash {scriptPath} # {jobName}";

            string currentCrontab = "";
            try
            {
                currentCrontab = RunCommand("crontab -l");
            }
            catch{}

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

            var newCrontab = filtered.ToString();
            RunCommand("crontab -", newCrontab);
        }

        public static void RemoveBackupJob(string jobName)
        {
            string currentCrontab = RunCommand("crontab -l");
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

            var process = new Process { StartInfo = psi };
            process.Start();

            if (!string.IsNullOrEmpty(input))
            {
                process.StandardInput.WriteLine(input);
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
