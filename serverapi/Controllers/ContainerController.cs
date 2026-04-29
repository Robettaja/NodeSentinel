using Microsoft.AspNetCore.Mvc;
using serverapi.Models;
using serverapi.Managers;
using serverapi.Managers.Container;

namespace serverapi.Controllers
{
    [ApiController]
    [Route("container")]
    public class ContainerController : ControllerBase
    {

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] ContainerData data)
        {
            if (await ContainerHandler.Handlers()[data.ServerType].Create(data, new CancellationToken()))
            {
                return Ok("Server created successfully");

            }
            return BadRequest("Server with this name already exist");

        }

        [HttpPost("{serverName}/start")]
        public async Task<IActionResult> Start(string serverName)
        {
            await ContainerHandler.Start(serverName, new CancellationToken());
            return Ok();
        }

        [HttpPost("{serverName}/restart")]
        public async Task<IActionResult> Restart(string serverName)
        {
            await ContainerHandler.Restart(serverName, new CancellationToken());
            return Ok();
        }
        [HttpPost("{serverName}/stop")]
        public async Task<IActionResult> Stop(string serverName)
        {
            await ContainerHandler.Stop(serverName, new CancellationToken());
            return Ok();
        }

        [HttpPost("{serverName}/command")]
        [ProducesResponseType<string>(StatusCodes.Status200OK, "application/json")]
        public async Task<IActionResult> Command(string serverName, [FromBody] string command, [FromQuery] ServerType type)
        {
            string data = await ContainerHandler.Command(serverName, command, type, new CancellationToken());
            return Ok(data);
        }

        [HttpGet("{serverName}/logs")]
        [ProducesResponseType<string>(StatusCodes.Status200OK, "application/json")]
        public async Task<IActionResult> Logs(string serverName)
        {
            string logs = await ContainerHandler.Logs(serverName, new CancellationToken());

            return Ok(logs);
        }
        [HttpGet("{serverName}/logs/stream")]
        public async Task StreamLogs(string serverName)
        {
            await ContainerHandler.StreamLogs(serverName, Response, "50", HttpContext.RequestAborted);
        }

        [HttpGet("{serverName}/status")]
        public async Task<ActionResult<string>> Status(string serverName)
        {
            string status = await ContainerHandler.Status(serverName, new CancellationToken());
            return Ok(status);
        }
        [HttpGet("{serverName}/stats")]
        public async Task<ActionResult<string>> Stats(string serverName)
        {
            SystemData status = await ContainerHandler.Stats(serverName, new CancellationToken());
            return Ok(status);
        }
        [HttpGet("availableport")]

        [HttpPost("{serverName}/edit")]
        public async Task<IActionResult> Edit(string serverName, ContainerData data)
        {
            await ContainerHandler.Handlers()[data.ServerType].Edit(data, serverName, new CancellationToken());
            return Ok();
        }

        [HttpDelete("{serverName}/delete")]
        public async Task<IActionResult> Delete(string serverName)
        {
            await ContainerHandler.Delete(serverName, new CancellationToken());
            return Ok();

        }
    }
}
