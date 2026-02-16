using TestBlazor.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContext<TestBlazor.Data.ToDoDbContext>(options =>
    options.UseSqlite("Data Source=todo_archive.db"));

builder.Services.AddScoped<TestBlazor.Services.ToDoService>();
builder.Services.AddSingleton<TestBlazor.Services.CompassService>();
builder.Services.AddSingleton<TestBlazor.Services.SimulationService>();
builder.Services.AddScoped<TestBlazor.Services.WcfService>();
builder.Services.AddSingleton<TestBlazor.Services.IUdpListenerService, TestBlazor.Services.UdpListenerService>(); // Registered as Singleton for continuous listening

var app = builder.Build();

// Ensure the database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TestBlazor.Data.ToDoDbContext>();
    dbContext.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
