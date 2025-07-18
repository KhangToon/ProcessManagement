using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using ProcessManagement.Data;
using ProcessManagement.Models;
using Microsoft.AspNetCore.Identity;
using Radzen;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using ProcessManagement.Services.SQLServer;
using ParamountBed_Warehouse.Services.SocketService;
using ProcessManagement.Services.QRCodes;
using System.Net;
using ProcessManagement.Services.Modbus;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Diagnostics;
using ProcessManagement.Commons;
using Microsoft.AspNetCore.Authentication.Cookies;

var localIP = PortFinder.GetLocalIPAddress();
IPAddress localIPAddress = IPAddress.Parse(localIP);
var port = PortFinder.FindAvailablePort_CheckUsed(localIPAddress);
var url = $"http://{localIP}:{port}";
// enable khi deploy - disable khi use local 


var builder = WebApplication.CreateBuilder(args);

// Identity config
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DBConnectionString") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found."); ;

    options.UseSqlServer(connectionString);
});

// Identity config (auto create after scaffold) // using when use default identity
builder.Services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 3;
    options.Password.RequiredUniqueChars = 1;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;

    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
});

// For API 
builder.Services.AddControllers();
// For API 
builder.Services.AddHttpClient(Common.ServerAPI, client =>
{
    client.BaseAddress = new Uri($"{url}/"); // Replace with your API URL
});
// For API 
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
    .CreateClient(Common.ServerAPI));

// Add Radzen components
builder.Services.AddRadzenComponents();
// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
// Blazor bootrstap
builder.Services.AddBlazorBootstrap();
// Radzen services
builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<TooltipService>();
builder.Services.AddScoped<ContextMenuService>();

// Authentication
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

// DB services
builder.Services.AddSingleton<SQLServerServices>();
// Socket services
builder.Services.AddSingleton<ServerSocketAsync>();
// QRCreate services
builder.Services.AddSingleton<QRCodeServices>();
// Modbus services
builder.Services.AddSingleton<ModbusServices>();

// Add ViTriofTPhamApiService
builder.Services.AddScoped<ViTriofTPhamApiService>();

// CRS (Cross-Origin Resource Sharing) - Configures access to the API from any origin
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

// Add authen/author services
app.UseAuthentication();
app.UseAuthorization();

// For API 
app.MapControllers(); // Map API controllers
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// Enable CORS - configures access to the API from any origin
app.UseCors();

// Open browser after application starts
app.Urls.Add(url); // enable khi deploy - disable khi use local  
app.Lifetime.ApplicationStarted.Register(() =>  // enable khi deploy - disable khi use local 
{
    var psi = new ProcessStartInfo
    {
        FileName = url,
        UseShellExecute = true
    };
    Process.Start(psi);
});
///////////


app.Run();


public static class PortFinder
{
    // Find an available port
    public static int FindAvailablePort_notcheckUsed(int startingPort = 5000)
    {
        var properties = IPGlobalProperties.GetIPGlobalProperties();
        var usedPorts = properties.GetActiveTcpListeners().Select(l => l.Port).ToList();

        for (int port = startingPort; port < 65535; port++)
        {
            if (!usedPorts.Contains(port))
            {
                return port;
            }
        }

        throw new Exception("No available ports found.");
    }


    public static int FindAvailablePort_CheckUsed(IPAddress? targetIP = null, int startingPort = 5000)
    {
        if (startingPort < 1 || startingPort > 65535)
            throw new ArgumentOutOfRangeException(nameof(startingPort), "Port must be between 1 and 65535");

        for (int port = startingPort; port <= 65535; port++)
        {
            if (IsPortAvailable(targetIP, port))
                return port;
        }

        throw new InvalidOperationException("No available ports found in range.");
    }

    private static bool IsPortAvailable(IPAddress? targetIP, int port)
    {
        // If targetIP is null, use IPAddress.Any
        var ipAddress = targetIP ?? IPAddress.Any;

        var tcpListener = new TcpListener(ipAddress, port);

        try
        {
            tcpListener.Start();
            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            tcpListener.Stop();
        }
    }

    // enable khi deploy - disable khi use local 
    // Get local IP address
    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }
}