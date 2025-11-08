using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Npgsql;

namespace BackupMonitor.Services;

public class PostgresService
{
    public string BackupDatabase(string host, int port, string dbName, string user, string password, string outputFile)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputFile)!);

        var psi = new ProcessStartInfo
        {
            FileName = "pg_dump",
            Arguments = $"-h {host} -p {port} -U {user} -F c -f \"{outputFile}\" {dbName}",
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        psi.Environment["PGPASSWORD"] = password;

        using var process = Process.Start(psi)!;
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
            throw new Exception($"Erro ao gerar dump: {error}");

        return outputFile;
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