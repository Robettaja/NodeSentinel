using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using web_server.Models;

namespace web_server.Controllers;

public class ServerController : Controller
{
    [Route("new server")]
    public async Task<IActionResult> Index()
    {
        return View();
    }

}
