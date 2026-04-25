using System.Text;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace client.Models
{
    public class ContainerHandler
    {
        private DockerClient client;
        public ContainerHandler()
        {
            client = new DockerClientConfiguration(
                new Uri("unix:///var/run/docker.sock")
            ).CreateClient();
        }

        public async Task<bool> Create(ContainerData containerData, CancellationToken ct)
        {

            ContainerListResponse? container = await GetByName(containerData.ServerName, ct);
            if (container != null) return false;
            HostConfig hostConfig = new HostConfig()
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
            {
                {
                    $"{containerData.Port}/tcp",
                    new List<PortBinding> { new PortBinding { HostPort = containerData.Port } }
                },

            },
                Binds = [$"{AppContext.BaseDirectory + containerData.ServerName}:/data"]


            };
            CreateContainerParameters parameters = new()
            {
                Name = containerData.ServerName,
                Tty = containerData.Tty,
                Image = containerData.Image,
                AttachStdin = containerData.AttachStdin,
                HostConfig = hostConfig,
                Env = containerData.Env,


            };
            await client.Containers.CreateContainerAsync(parameters, ct);
            return true;

        }
        public async Task Start(string name, CancellationToken ct)
        {
            ContainerListResponse? container = await GetByName(name, ct);
            if (container != null)
            {
                await client.Containers.StartContainerAsync(container.ID, null, ct);
            }

        }
        public async Task Restart(string name, CancellationToken ct)
        {
            ContainerListResponse? container = await GetByName(name, ct);
            if (container != null)
            {
                await client.Containers.RestartContainerAsync(container.ID, null, ct);
            }

        }
        public async Task Stop(string name, CancellationToken ct)
        {
            ContainerListResponse? container = await GetByName(name, ct);
            if (container != null)
            {
                await client.Containers.StopContainerAsync(container.ID, null, ct);
            }

        }
        public async Task<string> Logs(string name, CancellationToken ct)
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
        public async Task<ContainerStatsResponse?> Stats(string name, CancellationToken ct)
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
                return stats;
            }
            return null;

        }
        public async Task<string> Status(string name, CancellationToken ct)
        {
            ContainerListResponse? container = await GetByName(name, ct);
            if (container != null)
            {
                return container.Status;
            }
            return "";

        }
        public async Task<string> ExecRconCli(string name, string command, CancellationToken ct)
        {
            var execParams = new ContainerExecCreateParameters
            {
                AttachStdin = true,
                AttachStdout = true,
                AttachStderr = true,
                Tty = false,
                Cmd = new[] { ServerTypeData.SPECIFICS[ServerType.MINECRAFT].CommandSender, command }
            };
            var exec = await client.Exec.ExecCreateContainerAsync(name, execParams, ct);
            using var stream = await client.Exec.StartAndAttachContainerExecAsync(exec.ID, false, ct);
            using var ms = new MemoryStream();
            await stream.CopyOutputToAsync(Stream.Null, ms, ms, ct);

            return Encoding.UTF8.GetString(ms.ToArray()).Trim();

        }
        public async Task Delete(string name, CancellationToken ct)
        {
            ContainerListResponse? container = await GetByName(name, ct);
            if (container != null)
            {
                await client.Containers.RemoveContainerAsync(container.ID, null, ct);
            }

        }
        public async Task<ContainerListResponse?> GetByName(string name, CancellationToken ct)
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
