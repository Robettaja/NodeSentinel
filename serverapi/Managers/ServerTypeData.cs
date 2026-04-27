using System.Text;
using Docker.DotNet.Models;


namespace serverapi.Managers
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
            {ServerType.VALHEIM,new("supervisorctl","2456","ghcr.io/community-valheim-tools/valheim-server","/config")},

        };

    }
    public enum ServerType
    {
        TERRARIA,
        TMODLOADER,
        MINECRAFT,
        VALHEIM
    }

}
