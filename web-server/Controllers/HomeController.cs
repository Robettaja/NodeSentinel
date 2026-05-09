using System.Diagnostics;
using System.Net.NetworkInformation;
using KiotaPosts.Client;
using Microsoft.AspNetCore.Antiforgery;
using KiotaPosts.Client.Container.Item.Delete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using web_server.Managers;
using web_server.Models;
using web_server.Models.Tables;

namespace web_server.Controllers;

public class HomeController : Controller
{
    private readonly PostsClient _client;
    private readonly IConfiguration _config;
    public HomeController(IConfiguration config, PostsClient client)
    {
        _config = config;
        _client = client;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? serverId)
    {
        if (User.Identity!.IsAuthenticated)
        {
            User user = await DatabaseManipulator.GetSingle<User>(u => u.Username == User.Identity!.Name);
            var servers = await DatabaseManipulator.GetMany<Server>(s => s.UserID.Equals(user.Id));
            Server? selected = serverId != null
                ? await DatabaseManipulator.GetSingle<Server>(s => s.Id == ObjectId.Parse(serverId))
                : servers?.FirstOrDefault();
            ServerViewModel vm = new(_client)
            {
                Servers = servers,
                ActiveServer = selected,
            };
            if (selected != null)
            {
                vm.ServerUrl = $"{_config["PostsApi:Ip"]}:{selected.Port}";
            } 
            vm.ActiveServerStatus = await vm.GetStatus(selected);
            return View(vm);
        }
        return View(new ServerViewModel(_client));
    }
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Start(string? serverName)
    {
        
        Server server = await DatabaseManipulator.GetSingle<Server>(s => s.ServerName == serverName);
        await _client.Container[serverName].Start.PostAsync();
        return RedirectToAction("Index", "Home", new {serverId = server.Id.ToString()});

    }
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Restart(string? serverName)
    {
        Server server = await DatabaseManipulator.GetSingle<Server>(s => s.ServerName == serverName);
        await _client.Container[serverName].Restart.PostAsync();
        return RedirectToAction("Index", "Home", new {serverId = server.Id.ToString()});
    }
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Stop(string? serverName)
    {
        
        Server server = await DatabaseManipulator.GetSingle<Server>(s => s.ServerName == serverName);
        await _client.Container[serverName].Stop.PostAsync();
        return RedirectToAction("Index", "Home", new {serverId = server.Id.ToString()});
    }
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Stats(string? serverName)
    {
        try
        {
            var stats = await _client.Container[serverName].Stats.GetAsync();
            return Json(stats);

        }
        catch
        {
            
        }

        return Json(null);

    }

    [Authorize]
    public async Task<IActionResult> Command(string? serverName, string? command)
    {
        Server server = await DatabaseManipulator.GetSingle<Server>(s => s.ServerName == serverName);
        
        string response = await _client.Container[serverName].Command.PostAsync(command, config =>
        {
            config.QueryParameters.Type = (int)server.ServerType;
            
        }) ;
        return Content(response,"text/plain");
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Delete(string? serverName)
    {
        await DatabaseManipulator.DeleteOne<Server>(s => s.ServerName.Equals(serverName));
        await _client.Container[serverName].DeletePath.DeleteAsync();
        return RedirectToAction("Index", "Home");

    }

}
