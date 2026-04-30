using KiotaPosts.Client;
using Microsoft.AspNetCore.Mvc.Rendering;
using web_server.Models.Tables;

namespace web_server.Models
{
    public class DashboardViewModel
    {
        private PostsClient client;
        public List<Server>? Servers { get; set; }
        public Server ActiveServer { get; set; }
        public string ActiveServerStatus { get; set; }
        public DashboardViewModel(PostsClient client)
        {
            this.client = client;
        }
        public SelectList GetServerSelectList()
        {
            var items = Servers?.Select(s => new { Id = s.Id.ToString(), s.ServerName }) ?? [];
            return new SelectList(items, "Id", "ServerName", ActiveServer?.Id.ToString());
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
