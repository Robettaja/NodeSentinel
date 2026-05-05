using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace web_server.Controllers;

public class LogsController :  Controller
{
    [Authorize]
    public async Task<IActionResult> Index()
    {
        return View();
    }
    
}