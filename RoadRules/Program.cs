using Microsoft.EntityFrameworkCore;
using RoadRules.Components;
using RoadRules.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// A factory, not a plain AddDbContext. In Blazor Server a scoped DbContext
// would live for the whole user circuit; a factory gives each operation a
// short-lived context.
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

// SQLite creates the .db file but not its folder, so make sure App_Data exists.
Directory.CreateDirectory(Path.Combine(app.Environment.ContentRootPath, "App_Data"));

// Apply migrations on startup. Fine for a toy; in production migrations
// would run from the pipeline, not from the app.
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    using var db = factory.CreateDbContext();
    db.Database.Migrate();
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
