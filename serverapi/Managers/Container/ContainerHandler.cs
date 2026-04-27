using System.Text;
using serverapi.Models;
using Docker.DotNet;
using Docker.DotNet.Models;
namespace serverapi.Managers.Container
{
    public abstract class ContainerHandler
    {
        public static DockerClient client;
        static ContainerHandler()
        {
            client = new DockerClientConfiguration(
                new Uri("unix:///var/run/docker.sock")
            ).CreateClient();
        }
        public virtual async Task<bool> Create(ContainerData containerData, CancellationToken ct)
        {

            ContainerListResponse? container = await GetByName(containerData.ServerName, ct);
            if (container != null) return false;

            string image = ServerTypeData.SPECIFICS[containerData.ServerType].DefaultImage;
            await PullImage(image, ct);

            var serverSpec = ServerTypeData.SPECIFICS[containerData.ServerType];

            HostConfig hostConfig = new()
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
            {
                {
                    $"{serverSpec.DefaultPort}/tcp",
                    new List<PortBinding> { new() { HostPort = containerData.Port } }
                }
            },
                Binds = [$"{AppContext.BaseDirectory}servers/{containerData.ServerName}:{serverSpec.DataLocation}"]
            };

            CreateContainerParameters parameters = new()
            {
                Name = containerData.ServerName,
                Tty = containerData.Tty,
                Image = image,
                AttachStdin = containerData.AttachStdin,
                HostConfig = hostConfig,
                Env = containerData.Env,
            };

            var response = await client.Containers.CreateContainerAsync(parameters, ct);
            await client.Containers.StartContainerAsync(response.ID, new ContainerStartParameters(), ct);
            return true;
        }

        public static async Task<bool> Create2(ContainerData containerData, CancellationToken ct)
        {
            ContainerListResponse? container = await GetByName(containerData.ServerName, ct);
            if (container != null) return false;

            string image = ServerTypeData.SPECIFICS[containerData.ServerType].DefaultImage;
            await PullImage(image, ct);

            var serverSpec = ServerTypeData.SPECIFICS[containerData.ServerType];
            var portBindings = new Dictionary<string, IList<PortBinding>>();

            if (containerData.ServerType == ServerType.VALHEIM)
            {
                int basePort = int.Parse(containerData.Port);
                portBindings[$"{basePort}/udp"] = new List<PortBinding> { new() { HostPort = basePort.ToString() } };
                portBindings[$"{basePort + 1}/udp"] = new List<PortBinding> { new() { HostPort = (basePort + 1).ToString() } };
                portBindings[$"{basePort + 2}/udp"] = new List<PortBinding> { new() { HostPort = (basePort + 2).ToString() } };
            }
            else
            {
                portBindings[$"{serverSpec.DefaultPort}/tcp"] = new List<PortBinding> { new() { HostPort = containerData.Port } };
            }

            HostConfig hostConfig = new()
            {
                PortBindings = portBindings,
                Binds = [$"{AppContext.BaseDirectory + "servers/" + containerData.ServerName}:{serverSpec.DataLocation}"],
                CapAdd = containerData.ServerType == ServerType.VALHEIM
                    ? new List<string> { "sys_nice" }
                    : null
            };

            CreateContainerParameters parameters = new()
            {
                Name = containerData.ServerName,
                Tty = containerData.Tty,
                Image = image,
                AttachStdin = containerData.AttachStdin,
                HostConfig = hostConfig,
                Env = containerData.Env,
            };

            var response = await client.Containers.CreateContainerAsync(parameters, ct);
            await client.Containers.StartContainerAsync(response.ID, new ContainerStartParameters(), ct);
            return true;
        }
        protected static async Task PullImage(string image, CancellationToken ct)
        {
            try
            {
                await client.Images.InspectImageAsync(image, ct);
            }
            catch (DockerImageNotFoundException)
            {

                var parts = image.Split(':');
                var imageName = parts[0];
                var tag = parts.Length > 1 ? parts[1] : "latest";

                await client.Images.CreateImageAsync(
                    new ImagesCreateParameters
                    {
                        FromImage = imageName,
                        Tag = tag
                    },
                    null,
                    new Progress<JSONMessage>(msg =>
                    {
                    }),
                    ct
                );
            }
        }
        public static async Task Start(string name, CancellationToken ct)
        {
            ContainerListResponse? container = await GetByName(name, ct);
            if (container != null)
            {
                await client.Containers.StartContainerAsync(container.ID, null, ct);
            }

        }
        public static async Task Restart(string name, CancellationToken ct)
        {
            ContainerListResponse? container = await GetByName(name, ct);
            if (container != null)
            {
                await client.Containers.RestartContainerAsync(container.ID, new ContainerRestartParameters
                {
                    WaitBeforeKillSeconds = 10
                }, ct);
            }

        }
        public static async Task Stop(string name, CancellationToken ct)
        {
            ContainerListResponse? container = await GetByName(name, ct);
            if (container != null)
            {
                await client.Containers.StopContainerAsync(container.ID, new ContainerStopParameters
                {
                    WaitBeforeKillSeconds = 10
                }, ct);
            }

        }
        public static async Task<string> Logs(string name, CancellationToken ct)
        {

            ContainerListResponse? container = await GetByName(name, ct);
            if (container != null)
            {
                var parameters = new ContainerLogsParameters
                {
                    ShowStdout = true,
                    ShowStderr = true,
                    Tail = "100",
                    Timestamps = false
                };

                var stream = await client.Containers.GetContainerLogsAsync(name, true, parameters, ct);

                using var ms = new MemoryStream();
                await stream.CopyOutputToAsync(Stream.Null, ms, ms, ct);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
            return "";

        }
        public static async Task StreamLogs(string name, HttpResponse httpResponse, string tailAmount, CancellationToken ct)
        {
            ContainerListResponse? container = await GetByName(name, ct);
            if (container == null) return;

            httpResponse.ContentType = "text/event-stream";
            httpResponse.Headers.CacheControl = "no-cache";
            httpResponse.Headers["X-Accel-Buffering"] = "no";

            var parameters = new ContainerLogsParameters
            {
                ShowStdout = true,
                ShowStderr = true,
                Follow = true,
                Tail = tailAmount,
                Timestamps = false
            };

            var stream = await client.Containers.GetContainerLogsAsync(name, true, parameters, ct);
            var buffer = new byte[4096];

            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var result = await stream.ReadOutputAsync(buffer, 0, buffer.Length, ct);
                    if (result.EOF) break;

                    var text = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    await httpResponse.WriteAsync(text, ct);
                    await httpResponse.Body.FlushAsync(ct);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
        public static async Task<SystemData?> Stats(string name, CancellationToken ct)
        {
            ContainerListResponse? container = await GetByName(name, ct);
            if (container != null)
            {
                ContainerStatsResponse? stats = null;
                await client.Containers.GetContainerStatsAsync(
                    container.ID,
                    new ContainerStatsParameters { Stream = false },
                    new Progress<ContainerStatsResponse>(s => stats = s),
                    ct
                );
                return new(stats!);
            }
            return null;

        }
        public static async Task<string> Status(string name, CancellationToken ct)
        {
            ContainerListResponse? container = await GetByName(name, ct);
            if (container != null)
            {
                return container.Status;
            }
            return "";

        }
        public static async Task<string> Command(string name, string command, ServerType type, CancellationToken ct)
        {
            var execParams = new ContainerExecCreateParameters
            {
                AttachStdin = true,
                AttachStdout = true,
                AttachStderr = true,
                Tty = false,
                Cmd = [ServerTypeData.SPECIFICS[type].CommandSender, command]
            };
            var exec = await client.Exec.ExecCreateContainerAsync(name, execParams, ct);
            using var stream = await client.Exec.StartAndAttachContainerExecAsync(exec.ID, false, ct);
            using var ms = new MemoryStream();
            await stream.CopyOutputToAsync(Stream.Null, ms, ms, ct);

            return Encoding.UTF8.GetString(ms.ToArray()).Trim();

        }
        public virtual async Task Edit(ContainerData data, string oldName, CancellationToken ct)
        {
            ContainerListResponse? container = await GetByName(oldName, ct);
            if (container == null) return;

            await Stop(oldName, ct);
            await client.Containers.RemoveContainerAsync(container.ID, new ContainerRemoveParameters { Force = true }, ct);

            if (oldName != data.ServerName)
            {
                var oldPath = PathManager.GetServerPath(oldName);
                if (Directory.Exists(oldPath))
                {
                    var moveContainer = await client.Containers.CreateContainerAsync(new CreateContainerParameters
                    {
                        Image = "alpine",
                        Cmd = ["sh", "-c", $"mv /host/servers/{oldName} /host/servers/{data.ServerName}"],
                        HostConfig = new HostConfig
                        {
                            Binds = [$"{AppContext.BaseDirectory}:/host"],
                            AutoRemove = true
                        }
                    }, ct);

                    bool started = await client.Containers.StartContainerAsync(moveContainer.ID, new ContainerStartParameters(), ct);
                    await client.Containers.WaitContainerAsync(moveContainer.ID, ct);
                }
            }

            await Create(data, ct);
        }
        public static async Task Delete(string name, CancellationToken ct)
        {
            ContainerListResponse? container = await GetByName(name, ct);
            if (container != null)
            {
                await Stop(name, ct);
                await client.Containers.RemoveContainerAsync(container.ID, new ContainerRemoveParameters
                {
                    RemoveVolumes = true,
                    Force = true
                }, ct);

                string basePath = AppContext.BaseDirectory;

                CreateContainerResponse cleanup = await client.Containers.CreateContainerAsync(new CreateContainerParameters
                {
                    Image = "alpine",
                    Cmd = ["sh", "-c", $"rm -rf /host/servers/{name} /host/backups/{name}"],
                    HostConfig = new HostConfig
                    {
                        Binds = [$"{basePath}:/host"],
                        AutoRemove = true
                    }
                }, ct);

                await client.Containers.StartContainerAsync(cleanup.ID, new ContainerStartParameters(), ct);
                await client.Containers.WaitContainerAsync(cleanup.ID, ct);

            }

        }
        public static async Task<ContainerListResponse?> GetByName(string name, CancellationToken ct)
        {
            var containers = await client.Containers.ListContainersAsync(
                new ContainersListParameters
                {
                    All = true,
                    Filters = new Dictionary<string, IDictionary<string, bool>>
                    {
                        { "name", new Dictionary<string, bool> { { name, true } } }
                    }
                }, ct);
            return containers.FirstOrDefault();
        }
        public static IReadOnlyDictionary<ServerType, ContainerHandler> Handlers()
        {
            return new Dictionary<ServerType, ContainerHandler>(){
                {ServerType.TERRARIA,new TerrariaHandler()},
                {ServerType.TMODLOADER,new TmodHandler()},
                {ServerType.MINECRAFT,new MinecraftHandler()},
                {ServerType.VALHEIM,new ValheimHandler()},
            };
        }
    }
}
