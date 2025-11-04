using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Npgsql;

namespace BackupMonitor.Services;

public class PostgresService
{
    public string BackupDatabase(string host, int port, string dbName, string username,
        string password, string outputDirectory = null)
    {
        if (string.IsNullOrEmpty(host)) host = "localhost";
        if (port <= 0) port = 5432;
        if (string.IsNullOrEmpty(outputDirectory)) outputDirectory = Path.GetTempPath();

        Directory.CreateDirectory(outputDirectory);

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"{dbName}_{timestamp}.dump";
        string outputPath = Path.Combine(outputDirectory, fileName);

        string args = $"-h {EscapeArg(host)} -p {port} -U {EscapeArg(username)} " +
                      $"-F c -Z 9 -f \"{outputPath}\" {EscapeArg(dbName)}";

        var psi = new ProcessStartInfo
        {
            FileName = "pg_dump",
            Arguments = args,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        if (!string.IsNullOrEmpty(password))
            psi.EnvironmentVariables["PGPASSWORD"] = password;

        using (var proc = new Process { StartInfo = psi })
        {
            proc.Start();

            string stderr = proc.StandardError.ReadToEnd();
            proc.WaitForExit();

            if (proc.ExitCode != 0)
            {
                if (File.Exists(outputPath))
                    File.Delete(outputPath);

                throw new Exception($"pg_dump falhou: {stderr}");
            }
        }

        return outputPath;
    }

    private static string EscapeArg(string arg)
    {
        if (string.IsNullOrEmpty(arg)) return arg;
        return arg.Contains(' ') ? $"\"{arg.Replace("\"", "\\\"")}\"" : arg;
    }


    public bool TestarConexao(string host, int port, string username, string password, string dbName)
    {
        var connString = $"Host={host};Port={port};Username={username};Password={password};Database={dbName};";
        try
        {
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();
                conn.Close();
                return true;
            }
        }
        catch
        {
            return false;
        }
    }
}