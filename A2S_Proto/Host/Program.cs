using A2S_Proto;
using Host.Services;

// Fixing the Single-file temporary BasePath
if (Path.GetDirectoryName(Environment.ProcessPath) is string basePath)
    Directory.SetCurrentDirectory(basePath);

var builder = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
    .UseContentRoot(Directory.GetCurrentDirectory());

builder.ConfigureAppConfiguration((app, config) =>
{
    config.SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile(@"appSettings.json", true, true);
    if (app.HostingEnvironment.IsDevelopment())
    {
        config.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(@"appSettings.Development.json", true, true);
    }
    else if (app.HostingEnvironment.IsStaging())
    {
        config.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(@"appSettings.Staging.json", true, true);
    }

    config.AddEnvironmentVariables()
        .AddCommandLine(args);
})
.UseSerilog((app, config) => config.ReadFrom.Configuration(app.Configuration))
.ConfigureServices((host, services) =>
{
    services.AddHostedService<Worker>();
})
.UseCommandLine()
.UseA2S_Proto();

var app = builder.Build();

app.Services.GetRequiredService<ILogger<Program>>().LogInformation("Application is starting...");

await app.RunAsync();
