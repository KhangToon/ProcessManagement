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

var builder = WebApplication.CreateBuilder(args);

//builder.WebHost.UseKestrel(serveroption =>
//{
//    serveroption.Listen(IPAddress.Loopback, port: 5002);
//});

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

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
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

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

//app.Urls.Add("http://192.168.1.67:5000");

app.Run();
