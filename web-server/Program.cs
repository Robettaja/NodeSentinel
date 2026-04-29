using KiotaPosts.Client;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using web_server.Managers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

builder.Services.AddSingleton(sp =>
{
    var httpClient = sp.GetRequiredService<HttpClient>();

    var authProvider = new AnonymousAuthenticationProvider();

    var adapter = new HttpClientRequestAdapter(
        authProvider,
        httpClient: httpClient
    );

    adapter.BaseUrl = "http://localhost:5106";

    return new PostsClient(adapter);
});


DatabaseManipulator.Initialize(builder.Configuration);
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(option =>
        {
            option.LoginPath = "/";
            option.LogoutPath = "/Home/Index";
            option.AccessDeniedPath = "/Home/Index";
            option.ExpireTimeSpan = TimeSpan.FromDays(30);
            option.Cookie.Name = "NodeSentinel";
        });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
