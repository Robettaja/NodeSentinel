using MongoDB.Bson;

namespace web_server.Models.Tables
{
    public class Server : SaveableObject
    {
        public string? ServerName { get; set; }
        public bool Tty { get; set; }
        public bool AttachStdin { get; set; }
        public string? ServerType { get; set; }
        public string? Image { get; set; }
        public string? Port { get; set; }
        public Dictionary<string, string> Env { get; set; } = [];
        public ObjectId UserID { get; set; }
    }
}
