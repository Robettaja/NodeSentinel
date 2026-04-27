using client.Models;

namespace client.Managers.Backup
{
    public interface IBackupManager
    {
        public Task Create(string backupName, string save, string serverName);
        public Task<List<BackupItem>> List(string serverName);
        public Task Restore(string serverName, string backupName);
        public Task Delete(string serverName, string backupName);
        public static IReadOnlyDictionary<ServerType, IBackupManager> GetBackupManager()
        {
            return new Dictionary<ServerType, IBackupManager>
            {
                { ServerType.TERRARIA, new TerrariaBackupManager() },
                { ServerType.TMODLOADER, new TmodBackupManager() },
                { ServerType.MINECRAFT, new MinecraftBackupManager() },
                { ServerType.VALHEIM, new ValheimBackupManager() },
            };
        }

    }
}
