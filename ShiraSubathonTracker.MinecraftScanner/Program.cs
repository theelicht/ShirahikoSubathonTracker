using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShiraSubathonTracker.DAL;
using ShiraSubathonTracker.MinecraftScanner;

var builder = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(ConfigureServices);

var host = builder.Build();

AutoApplyMigrations(host);

host.Run();
return;

void ConfigureServices(HostBuilderContext context, IServiceCollection services)
{
    services.AddApplicationInsightsTelemetryWorkerService();
    services.ConfigureFunctionsApplicationInsights();

    services.AddScoped<StreamBuffer>();
    
    services.AddDbContext<TrackerDatabaseContext>(contextBuilder =>
        {
            contextBuilder.UseSqlServer(context.Configuration.GetConnectionString("DatabaseConnectionString"));
            contextBuilder.EnableSensitiveDataLogging();
        }
    );
}

void AutoApplyMigrations(IHost functionHost)
{
    var scope = functionHost.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetService<TrackerDatabaseContext>();
    dbContext!.Database.Migrate();
}