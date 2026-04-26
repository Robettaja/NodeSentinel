
namespace client.Models
{
    public class ContainerData
    {
        public string ServerName { get; set; } = string.Empty;
        public bool Tty { get; set; }
        public bool AttachStdin { get; set; }
        public ServerType ServerType { get; set; }
        public string? Port { get; set; }
        public List<string> Env { get; set; } = [];
    }
}
