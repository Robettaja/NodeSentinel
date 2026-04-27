namespace serverapi.Managers
{
    public static class PathManager
    {
        public static string GetBackupPath(string serverName) => Path.Combine(AppContext.BaseDirectory, "backups", serverName);
        public static string GetServerPath(string serverName) => Path.Combine(AppContext.BaseDirectory, "servers", serverName);
        public static string ValheimBackupPath(string serverName) => Path.Combine(AppContext.BaseDirectory, "servers", serverName, "config", "backups");

    }
}
