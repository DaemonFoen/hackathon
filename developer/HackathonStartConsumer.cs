using contracts;
using developer.Domain;
using MassTransit;

namespace developer;


public class HackathonStartConsumer
    : IConsumer<HackathonStartEvent>
{
    private readonly Developer developer;
    private readonly AllDevs allDevs;
    private readonly IPublishEndpoint publishEndpoint;
    
    public HackathonStartConsumer(Developer developer, AllDevs allDevs, IPublishEndpoint publishEndpoint)
    {
        this.developer = developer;
        this.allDevs = allDevs;
        this.publishEndpoint = publishEndpoint;
    }

    
    public Task Consume(ConsumeContext<HackathonStartEvent> context)
    {
        Console.WriteLine(developer);
        Console.WriteLine("--- MASSAGE CONSUME ---");
            
        var state = new WorkerState.Ready(developer, allDevs);
        var generator = new PreferencesGenerator(developer, allDevs);
        var preferences = generator.GenerateRandomSortedPreferences();
        var preferencesResponse = new Preferences(context.Message.HackathonId,
            state.Developer, preferences);
            
        Console.WriteLine($"Junior Received: ID {context.Message.HackathonId}");

        publishEndpoint.Publish(preferencesResponse);
        return Task.CompletedTask;
    }
}