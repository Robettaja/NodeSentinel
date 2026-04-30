using System.Diagnostics;
using System.Net.NetworkInformation;
using KiotaPosts.Client;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using web_server.Managers;
using web_server.Models;
using web_server.Models.Tables;

namespace web_server.Controllers;

public class HomeController(PostsClient client) : Controller
{
    private readonly PostsClient _client = client;

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
            DashboardViewModel vm = new(_client)
            {
                Servers = servers,
                ActiveServer = selected,
            };
            vm.ActiveServerStatus = await vm.GetStatus(selected);
            return View(vm);
        }
        return View(new DashboardViewModel(_client));
    }
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Start(string? serverName)
    {
        await _client.Container[serverName].Start.PostAsync();
        return RedirectToAction("Index", "Home");

    }
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Restart(string? serverName)
    {
        await _client.Container[serverName].Restart.PostAsync();
        return RedirectToAction("Index", "Home");
    }
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Stop(string? serverName)
    {
        Console.WriteLine(serverName);
        await _client.Container[serverName].Stop.PostAsync();
        return RedirectToAction("Index", "Home");
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

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
