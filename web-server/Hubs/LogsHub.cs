using System.Text;
using Microsoft.AspNetCore.SignalR;

namespace web_server.Hubs;
public class LogsHub : Hub
{
    private readonly string _baseUrl;
    private readonly string _apiKey;
    
    private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromHours(1) };

    public LogsHub(IConfiguration config)
    {
        _baseUrl = config["PostsApi:BaseUrl"];
        _apiKey = config["SecurityApi:ApiKey"];
    }
    
    public Task StreamLogs(string serverName)
    {
        var clientId = Context.ConnectionId;
        var clients = Clients;
        var ct = Context.ConnectionAborted;

        Task.Factory.StartNew(async () =>
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get,
                    $"{_baseUrl}/container/{serverName}/logs/stream");
                request.Headers.Add("SecurityApi", _apiKey);

                using var response = await _http.SendAsync(request,
                    HttpCompletionOption.ResponseHeadersRead, ct);

                await using var stream = await response.Content.ReadAsStreamAsync(ct);
                var buffer = new byte[256];

                while (!ct.IsCancellationRequested)
                {
                    int bytesRead = await stream.ReadAsync(buffer, ct);
                    if (bytesRead == 0) break;
                    var text = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    await clients.Client(clientId).SendAsync("ReceiveLog", text, ct);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Console.WriteLine($"StreamLogs error: {ex.Message}");
            }
        }, ct, TaskCreationOptions.LongRunning, TaskScheduler.Default);

        return Task.CompletedTask;
    }
}
