using BackupMonitor.Models;

namespace BackupMonitor.Services
{
    public static class SessionContext
    {
        public static AppConfig? CurrentConfig { get; set; }

        public static string? AccessPassword { get; set; }
    }
}