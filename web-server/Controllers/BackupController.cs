using System.Diagnostics;
using KiotaPosts.Client;
using KiotaPosts.Client.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using web_server.Managers;
using web_server.Models;
using web_server.Models.Tables;

namespace web_server.Controllers;

public class BackupController : Controller
{
    PostsClient _client;
    public BackupController(PostsClient client)
    {
        _client = client;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Index(string? serverId)
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
        await vm.GetBackups();
        vm.ActiveServerStatus = await vm.GetStatus(selected);
        return View(vm);

    }

    [Authorize]
    [Route("Backup/create")]
    [HttpPost]
    public async Task<IActionResult> CreateBackup(string? serverName, string? backupName)
    {
        Server server = await DatabaseManipulator.GetSingle<Server>(s => s.ServerName == serverName);
        string key = server.ServerType switch
        {
            GameSeverType.Terraria => "WORLDNAME",
            GameSeverType.Tmodloader => "TMOD_WORLDNAME",
            GameSeverType.Minecraft => "TBD",
            GameSeverType.Valheim => "TBD",

        };
        Console.WriteLine(server.ServerType);
        try
        {
            Console.WriteLine(server.Env[key]);
            var request = new KiotaPosts.Client.Models.CreateBackupRequest
            {
                BackupName = backupName,
                SaveSlot = server.Env[key],
                Type = (int)server.ServerType,
            };
            var result = await _client.Backup[serverName].Create.PostAsync(request);
        }
        catch (Exception ex)
        {
        }
        
        return RedirectToAction("Index", "Backup", new {serverId = server.Id.ToString()});
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> DeleteBackup(string? serverName, string? backupName)
    {
        User user = await DatabaseManipulator.GetSingle<User>(u => u.Username == User.Identity!.Name);
        Server server = await DatabaseManipulator.GetSingle<Server>(s => s.ServerName == serverName);
        
        if (server is null || server.UserID != user.Id)
            return Forbid();
        try
        {
            await _client.Backup[serverName].DeletePath.DeleteAsync(config =>
            {
                config.QueryParameters.Type = (int)server.ServerType;
                config.QueryParameters.BackupName = backupName;
            });
        }
        catch (Exception e)
        {
            
        }
        
        return RedirectToAction("Index", "Backup", new {serverId = server.Id.ToString()});
    }
    
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> RestoreBackup(string? serverName, string? backupName)
    {
        User user = await DatabaseManipulator.GetSingle<User>(u => u.Username == User.Identity!.Name);
        Server server = await DatabaseManipulator.GetSingle<Server>(s => s.ServerName == serverName);
        
        if (server is null || server.UserID != user.Id)
            return Forbid();
        try
        {
            await _client.Backup[serverName].Restore.PostAsync(config =>
            {
                config.QueryParameters.Type = (int)server.ServerType;
                config.QueryParameters.BackupName = backupName;
            });
        }
        catch (Exception e)
        {
            
        }
        return RedirectToAction("Index", "Backup", new {serverId = server.Id.ToString()});
    }
    
    
}
