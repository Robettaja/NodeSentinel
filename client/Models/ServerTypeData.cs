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
        public string DefaultImage { get; init; }
        public string DataLocation { get; init; }

        public ServerTypeData(string commandSender, string defaultPort, string defaultImage, string dataLocation)
        {
            CommandSender = commandSender;
            DefaultPort = defaultPort;
            DefaultImage = defaultImage;
            DataLocation = dataLocation;
        }
        public static IReadOnlyDictionary<ServerType, ServerTypeData> SPECIFICS = new Dictionary<ServerType, ServerTypeData>()
        {
            {ServerType.TERRARIA,new("inject","7777","passivelemon/terraria-docker:terraria-latest","/opt/terraria/config")},
            {ServerType.TMODLOADER,new("inject","7777","jacobsmile/tmodloader1.4:latest","/data")},
            {ServerType.MINECRAFT,new("rcon-cli","25565","itzg/minecraft-server:latest","/data")},
            {ServerType.VALHEIM,new("bash -c","2456","lloesche/valheim-server","/config")},

        };


    }

}
