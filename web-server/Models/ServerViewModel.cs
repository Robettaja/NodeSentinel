using KiotaPosts.Client;
using KiotaPosts.Client.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using web_server.Models.Tables;

namespace web_server.Models
{
    public class ServerViewModel
    {
        private PostsClient client;
        public List<Server>? Servers { get; set; }
        public string? ServerUrl{ get; set; }
        public Server ActiveServer { get; set; }
        public string ActiveServerStatus { get; set; }
        public List<BackupItem>? Backups { get; set; } = [];
        public ServerViewModel(PostsClient client)
        {
            this.client = client;
        }
        public SelectList GetServerSelectList()
        {
            var items = Servers?.Select(s => new { Id = s.Id.ToString(), s.ServerName }) ?? [];
            return new SelectList(items, "Id", "ServerName", ActiveServer?.Id.ToString());
        }

        public async Task GetBackups()
        {
            if(ActiveServer is null) return;
            try
            {
                Backups = await client.Backup[ActiveServer.ServerName].List.GetAsync(config =>
                {
                    config.QueryParameters.Type = (int)ActiveServer.ServerType;
                }) ?? [];

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public string GetServerImagePath(GameSeverType serverType)
        {
            GameSeverType type = serverType;
            return type switch
            {
                GameSeverType.Terraria =>"/Images/terraria.jpg",
                GameSeverType.Tmodloader =>"/Images/tmodloader.png",
                GameSeverType.Minecraft =>"/Images/minecraft.svg",
                GameSeverType.Valheim =>"/Images/valheim.png",
                _ => ""

            };

        }
        public async Task<string> GetStatus(Server server)
        {
            if (server?.ServerName is null) return "unknown";
            try
            {
                string? result = await client.Container[server.ServerName].Status.GetAsync();
                if (result is null) return "unknown";

                string lower = result.ToLower();

                if (lower.StartsWith("up"))
                    return $"Running for {result.Substring(2).Trim()}";

                if (lower.StartsWith("exited"))
                    return $"Stopped";

                if (lower.StartsWith("paused"))
                    return $"Stopped";

                if (lower.StartsWith("dead"))
                    return "Stopped";

                return result;
            }
            catch (HttpRequestException)
            {
                return "offline";
            }
        }

    }
    
}
