using KiotaPosts.Client;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using web_server.Hubs;
using web_server.Managers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/root/.aspnet/DataProtection-Keys"))
    .SetApplicationName("web-server");  // ← important: consistent name across restarts

builder.Services.AddHttpClient();
builder.Services.AddSingleton(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = new HttpClient(new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });


    var config = sp.GetRequiredService<IConfiguration>();
    var apiKey = config.GetValue<string>("SecurityApi:ApiKey");
    httpClient.DefaultRequestHeaders.Add("SecurityApi", apiKey);


    var adapter = new HttpClientRequestAdapter(
       new AnonymousAuthenticationProvider(),
       httpClient: httpClient
   )
    {
        BaseUrl = config["PostsApi:BaseUrl"]
    };
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

// app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAntiforgery();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapHub<LogsHub>("/logsHub");
app.Run();
