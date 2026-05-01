using Microsoft.AspNetCore.SignalR;

namespace web_server.Hubs;

public class LogsHub: Hub
{
    public async Task StreamLogs(string serverName)
    {
        using var http = new HttpClient();
        using var stream = await http.GetStreamAsync($"http://localhost:5106/container/{serverName}/logs/stream");
        using var reader = new StreamReader(stream);
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (line != null)
                await Clients.Caller.SendAsync("ReceiveLog", line);
        }
    }
    
}