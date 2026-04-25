using System.Text;
using Docker.DotNet.Models;

public enum ServerType
{
    TERRARIA,
    TMODLOADER,
    MINECRAFT,
    VALHEIM
}

namespace client.Models
{
    public class ServerTypeData
    {

        public string CommandSender { get; init; }
        public string DefaultPort { get; init; }

        public ServerTypeData(string commandSender, string defaultPort)
        {
            CommandSender = commandSender;
            DefaultPort = defaultPort;
        }
        public static IReadOnlyDictionary<ServerType, ServerTypeData> SPECIFICS = new Dictionary<ServerType, ServerTypeData>()
        {
            {ServerType.TERRARIA,new("inject","7777")},
            {ServerType.TMODLOADER,new("inject","7777")},
            {ServerType.MINECRAFT,new("rcon-cli","25565")},
            {ServerType.VALHEIM,new("bash -c","2456")},

        };


    }

}
