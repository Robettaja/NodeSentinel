using serverapi.Models;
using Docker.DotNet.Models;
using serverapi.Managers.Container;
using System.Formats.Tar;

namespace serverapi.Managers.Backup;

public class MinecraftBackupManager : IBackupManager
{

    public async Task Create(string backupName, string save, string serverName)
    {
        await ContainerHandler.Command(serverName, "save-all", ServerType.MINECRAFT, new CancellationToken());
        await Task.Delay(3000);
        string serverPath = PathManager.GetServerPath(serverName);
        string backupPath = PathManager.GetBackupPath(serverName);
        string backupFile = Path.Combine(backupPath, backupName);
        Directory.CreateDirectory(backupPath);

        TarFile.CreateFromDirectory(serverPath, backupFile + ".tar", false);

    }

    public async Task<List<BackupItem>> List(string serverName)
    {
        string backupPath = PathManager.GetBackupPath(serverName);

        if (!Directory.Exists(backupPath)) return [];

        return await Task.Run(() =>
            Directory.GetFiles(backupPath, "*.tar")
                .Select(f => new BackupItem
                {
                    Name = Path.GetFileName(f),
                    Size = (int)new FileInfo(f).Length,
                    CreatedAt = File.GetCreationTimeUtc(f)
                })
                .OrderByDescending(x => x.CreatedAt)
                .ToList()
        );
    }

    public async Task Restore(string serverName, string backupName)
    {
        string serverPath = PathManager.GetServerPath(serverName);
        string backupPath = PathManager.GetBackupPath(serverName);
        string backupFile = Path.Combine(backupPath, backupName + ".tar");

        if (!File.Exists(backupFile)) throw new Exception($"Backup '{backupName}' not found.");

        await ContainerHandler.Stop(serverName, new CancellationToken());
        Directory.Delete(serverPath);
        TarFile.ExtractToDirectory(backupFile, serverPath, false);

        await ContainerHandler.Start(serverName, new CancellationToken());
    }

    public async Task Delete(string serverName, string backupName)
    {
        string backupPath = PathManager.GetBackupPath(serverName);
        string backupFile = Path.Combine(backupPath, backupName + ".tar");

        if (!File.Exists(backupFile)) throw new Exception($"Backup '{backupName}' not found.");
        Directory.Delete(backupFile);


    }

}
