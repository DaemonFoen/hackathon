using MassTransit;

namespace hrmanager;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((_, config) => { config.AddEnvironmentVariables(); })
            .ConfigureServices((context, services) => { ConfigureServices(services, context.Configuration); });

        var host = builder.Build();
        host.Run();
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(_ => GetDirectorSettings(configuration));
        services.AddSingleton<TeamCreationService>(_ => new TeamCreationService(new TeamBuildingService()));
        
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
                
                cfg.ReceiveEndpoint(e =>
                {
                    e.Consumer<PreferencesResponseConsumer>(ctx);
                });
            });
        });
    }

    private static DirectorSettings GetDirectorSettings(IConfiguration configuration)
    {
        var uri = configuration["DIRECTOR_URI"] ?? throw new InvalidOperationException(
            "Переменная окружения 'DIRECTOR_URI' не установлена."
        );
        return new DirectorSettings(uri);
    }

}

public class DirectorSettings(string uri)
{
    public string Uri { get; } = uri;
}
