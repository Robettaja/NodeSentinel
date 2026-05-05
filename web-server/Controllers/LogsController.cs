using KiotaPosts.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using web_server.Managers;
using web_server.Models;
using web_server.Models.Tables;

namespace web_server.Controllers;


public class LogsController :  Controller
{
    
    private readonly PostsClient _client;
    
    public LogsController(PostsClient client)
    {
        _client = client;
    }
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Index(string? serverId)
    {
        User user = await DatabaseManipulator.GetSingle<User>(u => u.Username == User.Identity!.Name);
        var servers = await DatabaseManipulator.GetMany<Server>(s => s.UserID.Equals(user.Id)) ?? [];

        Server? selected = null;
        
        if (serverId != null && ObjectId.TryParse(serverId, out var objectId))
            selected = await DatabaseManipulator.GetSingle<Server>(s => s.Id == objectId);

        selected ??= servers.FirstOrDefault();

        ServerViewModel vm = new ServerViewModel(_client)
        {
            Servers = servers,
            ActiveServer = selected,
        };
        return View(vm);
    }
    
    
}