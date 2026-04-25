using Microsoft.AspNetCore.Mvc;
using client.Models;

namespace client.Controllers
{
    [ApiController]
    [Route("container")]
    public class ContainerController : ControllerBase
    {
        ContainerHandler containerHandler = new();

        [HttpPost("{id}/create")]
        public async Task<IActionResult> Create(string id, [FromBody] ContainerData data)
        {
            if (await containerHandler.Create(data, new CancellationToken()))
            {
                return BadRequest("Server with this name already exist");

            }
            Console.WriteLine("created container");
            return Ok("Server created successfully");

        }

        [HttpPost("{id}/start")]
        public async Task<IActionResult> Start(string id, [FromBody] string name)
        {
            await containerHandler.Start(name, new CancellationToken());
            return Ok();
        }

        [HttpPost("{id}/restart")]
        public async Task<IActionResult> Restart(string id, [FromBody] string name)
        {
            await containerHandler.Restart(name, new CancellationToken());
            return Ok();
        }
        [HttpPost("{id}/stop")]
        public async Task<IActionResult> Stop(string id, [FromBody] string name)
        {
            await containerHandler.Stop(name, new CancellationToken());
            return Ok();
        }
        [HttpPost("{id}/command")]
        public async Task<IActionResult> Command(string id, [FromBody] string name, [FromBody] string command)
        {
            string data = await containerHandler.ExecRconCli(name, command, new CancellationToken());
            return Ok(data);
        }

        [HttpGet("{id}/logs")]
        public async Task<IActionResult> Logs(string id, string name)
        {
            string logs = await containerHandler.Logs(name, new CancellationToken());

            return Ok(logs);
        }

        [HttpGet("{id}/status")]
        public async Task<IActionResult> Status(string id, string name)
        {
            string status = await containerHandler.Status(name, new CancellationToken());
            return Ok(status);
        }

        [HttpDelete("{id}/remove")]
        public async Task<IActionResult> Remove(string id, [FromBody] string name)
        {
            await containerHandler.Delete(name, new CancellationToken());
            return Ok();

        }
    }
}
