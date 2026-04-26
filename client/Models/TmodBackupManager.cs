using Docker.DotNet.Models;

namespace client.Models;

public class TmodBackupManager : IBackupManager
{

    public async Task Create(string backupName, string save, string serverName)
    {
        await ContainerHandler.Command(serverName, "save", ServerType.TERRARIA, new CancellationToken());
        await Task.Delay(3000);
        string serverPath = ((IBackupManager)this).GetServerPath(serverName);
        string backupPath = ((IBackupManager)this).GetBackupPath(serverName);
        Directory.CreateDirectory(backupPath);

        var response = await ContainerHandler.client.Containers.CreateContainerAsync(new CreateContainerParameters
        {
            Image = "alpine",
            Cmd = ["sh", "-c", $"tar -cf /backup/{backupName}.tar /source/tModLoader/Worlds/{save}.wld /source/tModLoader/Worlds/{save}.twld"],
            HostConfig = new HostConfig
            {
                Binds =
                    [
                        $"{serverPath}:/source",
                        $"{backupPath}:/backup"
                    ],
                AutoRemove = true
            }
        });
        await ContainerHandler.client.Containers.StartContainerAsync(response.ID, new ContainerStartParameters());
        await ContainerHandler.client.Containers.WaitContainerAsync(response.ID);
    }

    public async Task<List<BackupItem>> List(string serverName)
    {
        string backupPath = ((IBackupManager)this).GetBackupPath(serverName);

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
        string serverPath = ((IBackupManager)this).GetServerPath(serverName);
        string backupPath = ((IBackupManager)this).GetBackupPath(serverName);
        string backupFile = Path.Combine(backupPath, backupName);

        if (!File.Exists(backupFile)) throw new Exception($"Backup '{backupName}' not found.");

        await ContainerHandler.Stop(serverName, new CancellationToken());

        var response = await ContainerHandler.client.Containers.CreateContainerAsync(new CreateContainerParameters
        {
            Image = "alpine",
            Cmd = ["sh", "-c", $"tar -xf /backup/{backupName} -C /source/"],
            HostConfig = new HostConfig
            {
                Binds =
                [
                    $"{serverPath}:/source",
                $"{backupPath}:/backup"
                ],
                AutoRemove = true
            }
        });

        await ContainerHandler.client.Containers.StartContainerAsync(response.ID, new ContainerStartParameters());
        await ContainerHandler.client.Containers.WaitContainerAsync(response.ID);

        ContainerListResponse? container = await ContainerHandler.GetByName(serverName, new CancellationToken());
        if (container != null)
            await ContainerHandler.client.Containers.StartContainerAsync(container.ID, new ContainerStartParameters());
    }

    public async Task Delete(string serverName, string backupName)
    {
        string backupPath = ((IBackupManager)this).GetBackupPath(serverName);
        string backupFile = Path.Combine(backupPath, backupName);

        if (!File.Exists(backupFile)) throw new Exception($"Backup '{backupName}' not found.");

        await Task.Run(() => File.Delete(backupFile));
    }

}
