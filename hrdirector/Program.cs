using contracts;
using Hrdirector;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace hrdirector;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((_, config) => { config.AddEnvironmentVariables(); })
            .ConfigureServices((context, services) => { ConfigureServices(services, context.Configuration); });

        var host = builder.Build();
        InitDb(host);

        host.Run();
    }

    private static void InitDb(IHost host)
    {
        using var scope = host.Services.CreateScope();
        var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
        using var dbContext = dbContextFactory.CreateDbContext();
        dbContext.Database.EnsureCreated();
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<PreferencesResponseConsumer>();

            x.UsingRabbitMq((ctx, cfg) =>
            {
                var rabbitMqHost = configuration["RABBITMQ_HOST_NAME"] ??
                                   throw new InvalidOperationException(
                                       "RABBITMQ_HOST_NAME variable not defined.");
                var rabbitMqUser = configuration["RABBITMQ_USER"] ??
                                   throw new InvalidOperationException(
                                       "RABBITMQ_USER variable not defined.");
                var rabbitMqPassword = configuration["RABBITMQ_PASSWORD"] ??
                                       throw new InvalidOperationException(
                                           "RABBITMQ_PASSWORD variable not defined.");

                cfg.Host(rabbitMqHost, h =>
                {
                    h.Username(rabbitMqUser);
                    h.Password(rabbitMqPassword);
                });
                
                cfg.Publish<HackathonStartEvent>(c =>
                {
                    c.ExchangeType = "fanout";  
                });

                cfg.ReceiveEndpoint(e =>
                {
                    e.Consumer<PreferencesResponseConsumer>(ctx);
                });
            });
        });

        services.AddPooledDbContextFactory<ApplicationDbContext>(builder =>
            builder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
        );

        services.AddSingleton<HarmonyService>();
        services.AddHostedService<Director>();
    }
}
