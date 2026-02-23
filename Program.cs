using TestBlazor.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddDbContext<TestBlazor.Data.ToDoDbContext>(options =>
    options.UseSqlite("Data Source=todo_archive.db"));

builder.Services.AddScoped<TestBlazor.Services.ToDoService>();
builder.Services.AddSingleton<TestBlazor.Services.CompassService>();
builder.Services.AddSingleton<TestBlazor.Services.SimulationService>();
builder.Services.AddScoped<TestBlazor.Services.WcfService>();
builder.Services.AddSingleton<TestBlazor.Services.IUdpListenerService, TestBlazor.Services.UdpListenerService>(); // Registered as Singleton for continuous listening
builder.Services.AddTransient<TestBlazor.Services.IUdpSenderService, TestBlazor.Services.UdpSenderService>();
builder.Services.AddSingleton<TestBlazor.Client.Services.StopwatchService>();

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
else
{
    app.UseWebAssemblyDebugging();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<TestBlazor.Components.App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

// Serve the service worker manifest manually in development if necessary
app.MapGet("/service-worker-assets.js", async context =>
{
    var env = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
    var filePath = Path.Combine(env.ContentRootPath, "Client", "obj", "Debug", "net8.0", "service-worker-assets.js");
    
    // In publish, MapStaticAssets or UseStaticFiles handles this automatically.
    // In dev, the obj folder contains the generated manifest.
    if (File.Exists(filePath))
    {
        context.Response.ContentType = "application/javascript";
        await context.Response.SendFileAsync(filePath);
        return;
    }
    
    // Default empty fallback
    context.Response.ContentType = "application/javascript";
    await context.Response.WriteAsync("self.assetsManifest = { assets: [], version: '1' };");
});

app.Run();
