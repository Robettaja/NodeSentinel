using serverapi.Models;
using Docker.DotNet.Models;
using serverapi.Managers.Container;

namespace serverapi.Managers.Backup;

public class ValheimBackupManager : IBackupManager
{

    public async Task Create(string backupName, string save, string serverName)
    {
        string serverPath = PathService.GetServerPath(serverName, true);
        string backupPath = PathService.GetBackupPath(serverName, true);
        string sourcePath = Path.Combine(serverPath, "config", "worlds_local");

        Directory.CreateDirectory(backupPath);

        if (!Directory.Exists(sourcePath))
            throw new DirectoryNotFoundException($"Worlds directory not found: {sourcePath}");

        var response = await ContainerHandler.client.Containers.CreateContainerAsync(
            new CreateContainerParameters
            {
                Image = "alpine",
                Cmd = ["sh", "-c", $"tar -cf /backup/{backupName}.tar -C /source ."],
                HostConfig = new HostConfig
                {
                    Binds =
                    [
                        $"{sourcePath}:/source",
                    $"{backupPath}:/backup"
                    ],
                    AutoRemove = true
                }
            }
        );

        await ContainerHandler.client.Containers.StartContainerAsync(response.ID, new ContainerStartParameters());
        await ContainerHandler.client.Containers.WaitContainerAsync(response.ID);
    }

    public async Task<List<BackupItem>> List(string serverName)
    {
        string backupPath = PathService.GetBackupPath(serverName, false);

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

        string serverPath = PathService.GetServerPath(serverName, true);
        string backupPath = PathService.GetBackupPath(serverName, true);

        string backupFile = Path.Combine(backupPath, $"{backupName}");

        if (!File.Exists(backupFile))
            throw new FileNotFoundException("Backup not found", backupFile);

        await ContainerHandler.Stop(serverName, CancellationToken.None);

        string worldPath = Path.Combine(serverPath, "config", "worlds_local");


        var response = await ContainerHandler.client.Containers.CreateContainerAsync(
            new CreateContainerParameters
            {
                Image = "alpine",
                Cmd =
                [
                    "sh",
                "-c",
                $"tar -xf /backup/{backupName} -C /source"
                ],
                HostConfig = new HostConfig
                {
                    Binds =
                    [
                        $"{worldPath}:/source",
                    $"{backupPath}:/backup"
                    ],
                    AutoRemove = true
                }
            }
        );

        await ContainerHandler.client.Containers.StartContainerAsync(response.ID, new ContainerStartParameters());
        await ContainerHandler.client.Containers.WaitContainerAsync(response.ID);

        await ContainerHandler.Start(serverName, CancellationToken.None);
    }

    public async Task Delete(string serverName, string backupName)
    {

        string backupPath = PathService.GetBackupPath(serverName, false);

        string tarFile = Path.Combine(backupPath, $"{backupName}");

        if (File.Exists(tarFile))
            File.Delete(tarFile);

    }

}
