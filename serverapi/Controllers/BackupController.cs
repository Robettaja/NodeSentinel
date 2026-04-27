using serverapi.Managers;
using serverapi.Managers.Backup;
using serverapi.Models;
using Microsoft.AspNetCore.Mvc;

namespace serverapi.Controllers;

[ApiController]
[Route("backup")]
public class BackupController : Controller
{
    [HttpPost("{serverName}/create")]
    public async Task<IActionResult> Create(string serverName, [FromBody] CreateBackupRequest request)
    {
        await IBackupManager.GetBackupManager()[request.Type].Create(request.BackupName, request.SaveSlot, serverName);
        return Ok("Created backup");
    }
    [HttpGet("{serverName}/list")]
    public async Task<IActionResult> List(string serverName, [FromQuery] ServerType type)
    {
        List<BackupItem> item = await IBackupManager.GetBackupManager()[type].List(serverName);
        return Ok(item[0].Name);

    }
    [HttpPost("{serverName}/restore")]
    public async Task<IActionResult> Restore(string serverName, ServerType type, string backupName)
    {
        await IBackupManager.GetBackupManager()[type].Restore(serverName, backupName);
        return Ok();

    }
    [HttpDelete("{serverName}/delete")]
    public async Task<IActionResult> Delete(string serverName, ServerType type, string backupName)
    {
        await IBackupManager.GetBackupManager()[type].Delete(serverName, backupName);
        return Ok();

    }
}
public record CreateBackupRequest
{
    public string BackupName { get; set; } = string.Empty;
    public string SaveSlot { get; set; } = string.Empty;
    public ServerType Type { get; set; }
}
