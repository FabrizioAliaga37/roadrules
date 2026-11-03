using Microsoft.EntityFrameworkCore;
using RoadRules.Components;
using RoadRules.Data;
using RoadRules.Services;

var builder = WebApplication.CreateBuilder(args);

// Serve the build's static web assets (RoadRules.styles.css, the scoped CSS for the
// layout and nav) in every environment, not only Development. Without this, running
// the build outside Development (RoadRules.exe, --no-launch-profile) returns 404 for
// that file: no sidebar, and the hidden "An unhandled error has occurred" bar shows.
// Published output (Docker) doesn't need it and isn't affected.
builder.WebHost.UseStaticWebAssets();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// A factory, not a plain AddDbContext. In Blazor Server a scoped DbContext
// would live for the whole user circuit; a factory gives each operation a
// short-lived context.
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<ILimitSource, EfLimitSource>();
builder.Services.AddScoped<IWeightCalculator, WeightCalculator>();

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

// A full-page request to an unknown URL gets the "Page not found" page instead of a blank 404.
app.UseStatusCodePagesWithReExecute("/not-found");
app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
