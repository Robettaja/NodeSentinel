using System.Text;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace client.Models
{
    public static class ContainerHandler
    {
        public static DockerClient client;
        static ContainerHandler()
        {
            client = new DockerClientConfiguration(
                new Uri("unix:///var/run/docker.sock")
            ).CreateClient();
        }

        public static async Task<bool> Create(ContainerData containerData, CancellationToken ct)
        {

            ContainerListResponse? container = await GetByName(containerData.ServerName, ct);
            if (container != null) return false;
            string image = ServerTypeData.SPECIFICS[containerData.ServerType].DefaultImage;
            await PullImage(image, ct);
            HostConfig hostConfig = new()
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
            {
                {
                    $"{ServerTypeData.SPECIFICS[containerData.ServerType].DefaultPort}/tcp",
                    new List<PortBinding> { new() { HostPort = containerData.Port } }
                },

            },
                Binds = [$"{AppContext.BaseDirectory + "servers/" + containerData.ServerName}:/opt/terraria/config"]


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
            await client.Containers.CreateContainerAsync(parameters, ct);
            return true;

        }
        private static async Task PullImage(string image, CancellationToken ct)
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
                Cmd = new[] { ServerTypeData.SPECIFICS[type].CommandSender, command }
            };
            var exec = await client.Exec.ExecCreateContainerAsync(name, execParams, ct);
            using var stream = await client.Exec.StartAndAttachContainerExecAsync(exec.ID, false, ct);
            using var ms = new MemoryStream();
            await stream.CopyOutputToAsync(Stream.Null, ms, ms, ct);

            return Encoding.UTF8.GetString(ms.ToArray()).Trim();

        }
        public static async Task Edit(ContainerData data, string oldName, CancellationToken ct)
        {
            ContainerListResponse? container = await GetByName(oldName, ct);
            if (container != null)
            {
                Console.WriteLine("fsfd");
                await Stop(oldName, ct);
                Directory.Move($"{AppContext.BaseDirectory + oldName}", $"{AppContext.BaseDirectory + data.ServerName}");
                await client.Containers.RemoveContainerAsync(container.ID, new ContainerRemoveParameters
                {
                    RemoveVolumes = true,
                    Force = true
                }, ct);
                await Create(data, ct);

            }

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
    }
}
