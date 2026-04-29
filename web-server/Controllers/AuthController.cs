using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using web_server.Managers;
using web_server.Models;
using web_server.Models.Tables;

namespace web_server.Controllers;

public class AuthController : Controller
{

    [Route("/Account")]
    public async Task<IActionResult> Account()
    {
        return View(new AuthViewModel());
    }

    [Route("/Register")]
    public async Task<IActionResult> _Register()
    {
        return PartialView();
    }

    [Route("/login")]
    public async Task<IActionResult> _Login()
    {
        return PartialView();
    }


    [HttpPost]
    [Route("/login")]
    [RequireAntiforgeryToken]
    public async Task<IActionResult> Login(string username, string password)
    {
        if (!User.Identity!.IsAuthenticated)
        {
            User user = await DatabaseManipulator.GetSingle<User>(u => u.Username == username.Trim().ToLowerInvariant());
            if (user != null && PasswordHasher.Verify(password, user.Salt, user.Password))
            {
                Console.WriteLine("Login success");
                Authenticate(user.Username);
                return RedirectToAction("Index", "Home");
            }
            TempData["errors"] = "Username or password is incorrect";
        }
        return RedirectToAction("Account", "Auth");
    }

    [HttpPost]
    [Route("/Register")]
    [RequireAntiforgeryToken]
    public async Task<IActionResult> Register(AuthViewModel user)
    {
        if (!User.Identity!.IsAuthenticated)
        {

            if (user == null)
                return RedirectToAction("Account", "Auth", new { tab = "register" });
            if (ModelState.IsValid)
            {
                user.Username = user.Username.Trim().ToLowerInvariant();

                User duplicateUser = await DatabaseManipulator.GetSingle<User>(u => u.Username == user.Username);

                if (duplicateUser != null)
                {
                    TempData["errors"] = "User with this username already exists";
                    return RedirectToAction("Account", "Auth", new { tab = "register" });

                }
                byte[] salt = PasswordHasher.Salt(32);
                User newUser = new()
                {
                    Username = user.Username,
                    Salt = salt,
                    Password = PasswordHasher.Hash(user.Password, salt)
                };

                await DatabaseManipulator.Save(newUser);
                Authenticate(newUser.Username);

                return RedirectToAction("Index", "Home");
            }

        }
        return RedirectToAction("Account", "Auth", new { tab = "register" });

    }
    [Route("/Logout")]
    [HttpPost]
    [RequireAntiforgeryToken]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction("Index", "Home");
    }

    public void Authenticate(string Username)
    {
        List<Claim> claims = new() {
                new Claim(ClaimTypes.Name,Username)
            };
        ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        AuthenticationProperties authProperties = new()
        {
            AllowRefresh = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30),
            IsPersistent = true

        };
        HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(identity
                            ), authProperties);
    }
}
