using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// HttpClient for the browser, pointed at the server that served the app.
// The server has its own registration (RoadRules/Program.cs) for the prerender
// and InteractiveServer phases of CalculatorApi.
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

await builder.Build().RunAsync();
