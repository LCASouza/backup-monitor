using Avalonia;
using BackupMonitor.Services;
using System;

namespace BackupMonitor;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    static int Main(string[] args)
    {
        try
        {
            // Modo de execução automática (via cron)
            if (args.Length >= 2 && args[0] == "--auto-backup")
            {
                string tipo = args[1];
                string? filePath = null;

                if (args.Length >= 4 && args[2] == "--file")
                    filePath = args[3];

                string senha = Environment.GetEnvironmentVariable("BACKUPMONITOR_PASSWORD") ?? "sua_senha_aqui";

                var cfg = ConfigService.LoadConfig(senha);

                var auto = new AutoBackupService();
                auto.ExecutarBackupAutomatico(cfg, tipo, filePath);

                return 0; // finaliza normalmente
            }

            // 🧠 Só inicia Avalonia se não estiver em modo automático
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Erro fatal: {ex}");
            return 1;
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}