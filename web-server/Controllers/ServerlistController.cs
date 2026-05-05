using KiotaPosts.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using web_server.Managers;
using web_server.Models;
using web_server.Models.Tables;

namespace web_server.Controllers;

public class ServerlistController : Controller
{
    PostsClient _client;
    public ServerlistController(PostsClient client)
    {
        _client = client;
    }
    
    [Authorize]
    [Route("serverlist")]
    public async Task<IActionResult> Index()
    {
        User user = await DatabaseManipulator.GetSingle<User>(u => u.Username == User.Identity.Name);
        List<Server> servers = await DatabaseManipulator.GetMany<Server>(s => s.UserID == user.Id);
        ServerViewModel vm = new(_client)
        {
            Servers = servers
        };
        return View(vm);
    }
    
}