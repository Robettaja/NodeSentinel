using serverapi.Models;
using Docker.DotNet.Models;

namespace serverapi.Managers.Container
{
    public class TmodHandler : ContainerHandler
    {

        public override async Task<bool> Create(ContainerData containerData, CancellationToken ct)
        {
            ContainerListResponse? container = await GetByName(containerData.ServerName, ct);
            if (container != null) return false;

            var containers = await client.Containers.ListContainersAsync(new ContainersListParameters { All = true });

            var usedPorts = new List<int>();
            foreach (var c in containers)
            {
                var inspect = await client.Containers.InspectContainerAsync(c.ID);
                var ports = inspect.HostConfig.PortBindings?
                    .SelectMany(p => p.Value ?? [])
                    .Select(b => int.TryParse(b.HostPort, out var p) ? p : 0)
                    .Where(p => p > 0);

                if (ports != null)
                    usedPorts.AddRange(ports);
            }

            var serverSpec = ServerTypeData.SPECIFICS[containerData.ServerType];
            var port = PortFinder.GetNextAvailablePort(int.Parse(serverSpec.DefaultPort), usedPorts);

            containerData.Env.Add($"{serverSpec.PortEnv}=\"{port}\"");

            string image = serverSpec.DefaultImage;
            await PullImage(image, ct);

            HostConfig hostConfig = new()
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
        {
            {
                $"{serverSpec.DefaultPort}/tcp",
                new List<PortBinding> { new() { HostPort = port.ToString() } }
            }
        },
                Binds = [$"{PathService.GetServerPath(containerData.ServerName, true)}:{serverSpec.DataLocation}"],
                SecurityOpt = new List<string> { "seccomp=unconfined" }
            };

            CreateContainerParameters parameters = new()
            {
                Name = containerData.ServerName,
                Tty = containerData.Tty,
                Entrypoint = ["/bin/sh", "-c", "\"apt-get update && apt-get install -y libicu-dev && /terraria-server/entrypoint.sh\""],
                Image = image,
                AttachStdin = containerData.AttachStdin,
                HostConfig = hostConfig,
                Env = containerData.Env,
            };

            var response = await client.Containers.CreateContainerAsync(parameters, ct);
            await client.Containers.StartContainerAsync(response.ID, new ContainerStartParameters(), ct);
            return true;
        }
    }
}
