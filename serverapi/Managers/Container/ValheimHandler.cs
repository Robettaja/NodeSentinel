using serverapi.Models;
using Docker.DotNet.Models;

namespace serverapi.Managers.Container
{
    public class ValheimHandler : ContainerHandler
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
            int basePort = PortFinder.GetNextAvailablePort(int.Parse(serverSpec.DefaultPort), usedPorts);

            // Reserve 3 consecutive ports for Valheim
            while (usedPorts.Contains(basePort) || usedPorts.Contains(basePort + 1) || usedPorts.Contains(basePort + 2))
                basePort++;

            containerData.Env.Add($"{serverSpec.PortEnv}={basePort}");

            string image = serverSpec.DefaultImage;
            await PullImage(image, ct);

            HostConfig hostConfig = new()
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
        {
            { $"{basePort}/udp",     new List<PortBinding> { new() { HostPort = basePort.ToString() } } },
            { $"{basePort + 1}/udp", new List<PortBinding> { new() { HostPort = (basePort + 1).ToString() } } },
            { $"{basePort + 2}/udp", new List<PortBinding> { new() { HostPort = (basePort + 2).ToString() } } },

        },

                Binds =
                [
                    $"{AppContext.BaseDirectory}servers/{containerData.ServerName}/config:{serverSpec.DataLocation}",
            $"{AppContext.BaseDirectory}servers/{containerData.ServerName}/binaries:/opt/valheim"
                ],

                SecurityOpt = new List<string> { "seccomp=unconfined" },
                CapAdd = ["sys_nice"]
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
    }
}
