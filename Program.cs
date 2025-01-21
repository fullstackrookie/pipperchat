using PipperChat.Data;
using PipperChat.Controllers;
using PipperChat.Services;
using Microsoft.EntityFrameworkCore;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using  PipperChat.Models;
using System.Linq.Expressions;



var builder = WebApplication.CreateBuilder(args);

if  (builder.Environment.IsProduction())
{
    try
    {
        var KeyVaultUrl =   new     Uri(builder.Configuration["KeyVault:Url"]!); 
        builder.Configuration.AddAzureKeyVault(
        KeyVaultUrl,
        new DefaultAzureCredential()
        );

        //  Add logging to  track   Key vault   connection  status
        builder.Services.AddLogging(logging =>
        {
            logging.AddConsole();
            logging.AddDebug();
        });
    }
    catch   (Exception  ex)
    {
        Console.WriteLine($"Failed  to  connect to  Key Vault:  {ex.Message}");
        throw;
    }
}

// Add services and configurations 
builder.WebHost.UseUrls("http://127.0.0.1:5000");

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<PipperChatContext>(options    =>  options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllers();
builder.Services.AddScoped<PipperChat.Services.IPasswordService,    PipperChat.Services.PasswordService>();

// Add logging to the application
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();



var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
