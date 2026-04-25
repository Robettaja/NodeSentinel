
namespace client.Models
{
    public class ContainerData
    {
        public string ServerName { get; set; } = string.Empty;
        public bool Tty { get; set; }
        public bool AttachStdin { get; set; }
        public string? ServerType { get; set; }
        public string? Image { get; set; }
        public string? Port { get; set; }
        public List<string> Env { get; set; } = [];
    }
}
