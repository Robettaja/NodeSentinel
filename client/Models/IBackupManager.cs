namespace client.Models
{
    public interface IBackupManager
    {
        public Task Create(string backupName, string save, string serverName);
        public Task<List<BackupItem>> List(string serverName);
        public Task Restore(string serverName, string backupName);
        public Task Delete(string serverName, string backupName);
        public string GetBackupPath(string serverName) => Path.Combine(AppContext.BaseDirectory, "backups", serverName);
        public string GetServerPath(string serverName) => Path.Combine(AppContext.BaseDirectory, "servers", serverName);

    }
}
