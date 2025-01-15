using Microsoft.EntityFrameworkCore;
using PipperChat.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services and configurations 
builder.WebHost.UseUrls("http://127.0.0.1:5000");
// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<PipperChatContext>(options    =>  options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add logging to the application
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

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
