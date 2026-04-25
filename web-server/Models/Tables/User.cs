
namespace web_server.Models.Tables
{
    public class User : SaveableObject
    {
        public string? Username { get; set; }
        public byte[]? Password { get; set; }
        public byte[]? Salt { get; set; }
    }
}
