using Microsoft.AspNetCore.Components;
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
// Server and WebAssembly. Pages choose their own render mode: Calculator and
// Compare are InteractiveServer, CalculatorApi (in RoadRules.Client) is
// InteractiveAuto, and the read-only pages stay static.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// A factory, not a plain AddDbContext. In Blazor Server a scoped DbContext
// would live for the whole user circuit; a factory gives each operation a
// short-lived context.
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<ILimitSource, EfLimitSource>();
builder.Services.AddScoped<IWeightCalculator, WeightCalculator>();

// Lets the Vite dev server (a different origin) call the API during development.
builder.Services.AddCors(o => o.AddPolicy("vite", p =>
    p.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod()));

// HttpClient for components running on the server (CalculatorApi), pointed at this app.
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(sp.GetRequiredService<NavigationManager>().BaseUri)
});

var app = builder.Build();


// Apply migrations on startup. Fine for a toy; in production migrations
// would run from the pipeline, not from the app.
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    using var db = factory.CreateDbContext();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// A full-page request to an unknown URL gets the "Page not found" page instead of a blank 404.
app.UseStatusCodePagesWithReExecute("/not-found");
app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseCors("vite");
app.UseAntiforgery();

// Minimal API. The service throws; the API turns that into 404/400 with a JSON
// message, because an HTTP client can't catch a C# exception.
var today = () => DateOnly.FromDateTime(DateTime.Today);

app.MapGet("/api/jurisdictions", async (IDbContextFactory<AppDbContext> factory) =>
{
    await using var db = await factory.CreateDbContextAsync();
    return await db.Jurisdictions.OrderBy(j => j.Name)
        .Select(j => new { j.Code, j.Name }).ToListAsync();
});

app.MapGet("/api/limits/{code}", async (string code, DateOnly? asOf, IWeightCalculator calc) =>
{
    try { return Results.Ok(await calc.EffectiveLimitsAsync(code, asOf ?? today())); }
    catch (KeyNotFoundException ex) { return Results.NotFound(new { message = ex.Message }); }
});

app.MapGet("/api/calculate", async (string code, int axles, DateOnly? asOf, IWeightCalculator calc) =>
{
    try { return Results.Ok(await calc.CalculateAsync(code, axles, asOf ?? today())); }
    catch (KeyNotFoundException ex) { return Results.NotFound(new { message = ex.Message }); }
    catch (ArgumentOutOfRangeException ex) { return Results.BadRequest(new { message = ex.Message }); }
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(RoadRules.Client._Imports).Assembly);

app.Run();

// For WebApplicationFactory in the integration tests (step 15).
public partial class Program { }
