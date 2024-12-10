using System.Globalization;
using contracts;
using CsvHelper;
using CsvHelper.Configuration;
using developer.Domain;
using MassTransit;
using Type = contracts.Type;

namespace developer;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((_, config) => { config.AddEnvironmentVariables(); })
            .ConfigureServices((context, services) =>
            {
                ConfigureServices(services, context.Configuration);
            });

        var host = builder.Build();
        host.Run();
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var allDevs = LoadUsersFromCsv("Teamleads5.csv", "Juniors5.csv");

        var id = int.Parse(configuration["ID"] ??
                           throw new InvalidOperationException("ID variable not defined."));
        var type = configuration["TYPE"] ??
                   throw new InvalidOperationException("TYPE variable not defined.");
        var developer = type switch
        {
            "junior" => allDevs.Junior[id - 1],
            "teamlead" => allDevs.Teamlead[id - 1],
            _ => throw new NotSupportedException($"Type {type} not supported.")
        };

        services.AddSingleton(developer);
        services.AddSingleton(allDevs);
        
        services.AddMassTransit(x =>
        {
            x.AddConsumer<HackathonStartConsumer>();
            
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
                    e.ConfigureConsumer<HackathonStartConsumer>(ctx);
                });
                
                cfg.Publish<Preferences>(c =>
                {
                    c.ExchangeType = "fanout";  
                });
                

                
            });
        });

        Console.WriteLine($"Developer: {developer.Id}");
    }
    
    static AllDevs LoadUsersFromCsv(string teamLeadFilePath,
        string juniorFilePath)
    {
        var juniors = new List<Developer>();
        var teamLeads = new List<Developer>();

        using (var reader = new StreamReader(teamLeadFilePath))
        using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
               {
                   Delimiter = ";",
                   HasHeaderRecord = true
               }))
        {
            var records = csv.GetRecords<UserRecord>().ToList();
            foreach (var record in records)
            {
                teamLeads.Add(new Developer(record.Id, Type.TeamLead));
            }
        }

        using (var reader = new StreamReader(juniorFilePath))
        using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
               {
                   Delimiter = ";",
                   HasHeaderRecord = true
               }))
        {
            var records = csv.GetRecords<UserRecord>().ToList();
            foreach (var record in records)
            {
                juniors.Add(new Developer(record.Id, Type.Junior));
            }
        }

        Console.WriteLine(
            $"Loaded {teamLeads.Count + juniors.Count} users from {teamLeadFilePath} and {juniorFilePath}.");
        return new AllDevs(juniors, teamLeads);
    }
}
