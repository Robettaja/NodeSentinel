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
    public async Task<IActionResult> Terraria(TerrariaServerViewModel vm)
    {
        
        Server existing = await DatabaseManipulator.GetSingle<Server>(s => s.ServerName == vm.ServerName);
        if (existing != null)
        {
            ModelState.AddModelError("ServerName", "A server with this name already exists.");
            return View(vm);
        }
        if (ModelState.IsValid)
        {
            User user = await DatabaseManipulator.GetSingle<User>(u => u.Username == User.Identity!.Name);
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
    public async Task<IActionResult> Tmodloader(TmodServerViewModel vm)
    {
        
        Server existing = await DatabaseManipulator.GetSingle<Server>(s => s.ServerName == vm.ServerName);
        if (existing != null)
        {
            ModelState.AddModelError("ServerName", "A server with this name already exists.");
            return View(vm);
        }

        if (ModelState.IsValid)
        {
            User user = await DatabaseManipulator.GetSingle<User>(u => u.Username == User.Identity!.Name);
            Dictionary<string, string> EnvData = new()
            {
                { "TMOD_WORLDSEED", vm.Seed },
                { "TMOD_MOTD", vm.MOTD },
                { "TMOD_MAXPLAYERS", vm.MaxPlayers.ToString() },
                { "TMOD_LANGUAGE", vm.ToTerrariaLanguageCode(vm.Language) },
                { "TMOD_WORLDNAME", vm.WorldName },
                { "TMOD_DIFFICULTY", ((int)vm.Difficulty).ToString() },
                { "TMOD_WORLDSIZE", ((int)vm.WorldSize).ToString() },
                { "TMOD_PASS", vm.Password },
                { "TMOD_ENABLEDMODS", "" },
                { "TMOD_AUTODOWNLOAD", "" },
            };
            List<string> containerEnv = EnvData.Select(kvp => kvp.Key + "=" + kvp.Value).ToList();
            Server newServer = new()
            {
                ServerName = vm.ServerName,
                ServerType = GameSeverType.Tmodloader,
                UserID = user.Id,
                Env = EnvData,

            };
            await DatabaseManipulator.Save(newServer);

            ContainerData cd = new()
            {
                ServerName = vm.ServerName,
                ServerType = (int)GameSeverType.Tmodloader,
                AttachStdin = true,
                Tty = true,
                Env = containerEnv,
            };
            await _client.Container.Create.PostAsync(cd);
            return RedirectToAction("Index", "Home");

        }
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
