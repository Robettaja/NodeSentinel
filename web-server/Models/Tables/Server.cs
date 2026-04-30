using MongoDB.Bson;
using MongoDB.Driver.Core.Servers;

namespace web_server.Models.Tables
{
    public class Server : SaveableObject
    {
        public string? ServerName { get; set; }
        public GameSeverType ServerType { get; set; }
        public Dictionary<string, string> Env { get; set; } = [];
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public ObjectId UserID { get; set; }
    }
    public enum GameSeverType
    {
        Terraria,
        Tmodloader,
        Minecraft,
        Valheim

    }
}
