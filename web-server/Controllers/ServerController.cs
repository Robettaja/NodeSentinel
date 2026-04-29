using KiotaPosts.Client;
using KiotaPosts.Client.Models;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Kiota.Abstractions.Serialization;
using MongoDB.Driver.Core.Servers;
using web_server.Managers;
using web_server.Models;
using web_server.Models.Tables;

namespace web_server.Controllers;

public class ServerController : Controller
{
    private readonly PostsClient _client;

    public ServerController(PostsClient client)
    {
        _client = client;
    }
    [Route("new server")]
    [Authorize]
    public async Task<IActionResult> Index()
    {
        return View();
    }
    [Route("new server/terraria")]
    [Authorize]
    public async Task<IActionResult> Terraria()
    {
        return View(new TerrariaServerViewModel());
    }

    [HttpPost]
    [Authorize]
    [Route("new server/terraria")]
    [RequireAntiforgeryToken]
    public async Task<IActionResult> Terraria(TerrariaServerViewModel vm)
    {
        if (ModelState.IsValid)
        {
            User user = await DatabaseManipulator.GetSingle<User>(u => u.Username == User.Identity!.Name);
            List<Server> terrariaServers = await DatabaseManipulator.GetMany<Server>(s => s != null);
            Dictionary<string, string> EnvData = new() {
                {"SEED",vm.Seed},
                {"MOTD",vm.MOTD},
                {"MAXPLAYERS",vm.MaxPlayers.ToString()},
                {"LANGUAGE",vm.ToTerrariaLanguageCode(vm.Language)},
                {"WORLDNAME",vm.WorldName},
                {"SETDIFFICULTY",((int)vm.Difficulty).ToString()},
                {"AUTOCREATE",((int)vm.WorldSize).ToString()},
                {"PASSWORD",vm.Password},
            };
            List<string> containerEnv = EnvData.Select(kvp => kvp.Key + "=" + kvp.Value).ToList();
            Server newServer = new()
            {
                ServerName = vm.ServerName,
                ServerType = GameSeverType.Terraria,
                UserID = user.Id,
                Env = EnvData,

            };
            await DatabaseManipulator.Save(newServer);

            ContainerData cd = new()
            {
                ServerName = vm.ServerName,
                ServerType = (int)GameSeverType.Terraria,
                AttachStdin = true,
                Tty = true,
                Env = containerEnv,
            };
            await _client.Container.Create.PostAsync(cd);
            return RedirectToAction("Index", "Home");

        }


        return View();
    }
    [Route("new server/tmodloader")]
    [Authorize]
    public async Task<IActionResult> Tmodloader()
    {
        return View();
    }
    [Route("new server/minecraft")]
    [Authorize]
    public async Task<IActionResult> Minecraft()
    {
        return View();
    }
    [Route("new server/valheim")]
    [Authorize]
    public async Task<IActionResult> Valheim()
    {
        return View();
    }

}
