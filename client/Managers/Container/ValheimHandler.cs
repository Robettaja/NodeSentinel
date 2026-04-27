using client.Models;
using Docker.DotNet.Models;

namespace client.Managers.Container
{
    public class ValheimHandler : ContainerHandler
    {
        public override async Task<bool> Create(ContainerData containerData, CancellationToken ct)
        {
            ContainerListResponse? container = await GetByName(containerData.ServerName, ct);
            if (container != null) return false;

            string image = ServerTypeData.SPECIFICS[containerData.ServerType].DefaultImage;
            await PullImage(image, ct);

            var serverSpec = ServerTypeData.SPECIFICS[containerData.ServerType];
            int basePort = int.Parse(containerData.Port);

            HostConfig hostConfig = new()
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    { $"{basePort}/udp", new List<PortBinding> { new() { HostPort = basePort.ToString() } } },
                    { $"{basePort + 1}/udp", new List<PortBinding> { new() { HostPort = (basePort + 1).ToString() } } },
                    { $"{basePort + 2}/udp", new List<PortBinding> { new() { HostPort = (basePort + 2).ToString() } } }
                },
                Binds =
                [
                    $"{AppContext.BaseDirectory}servers/{containerData.ServerName}/config:{serverSpec.DataLocation}",
                    $"{AppContext.BaseDirectory}servers/{containerData.ServerName}/binaries:/opt/valheim"
                ],
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
