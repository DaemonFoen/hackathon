using System.Net;
using System.Text;
using System.Text.Json;
using contracts;
using MassTransit;

namespace hrmanager;

public class PreferencesResponseConsumer(
    DirectorSettings directorSettings,
    TeamCreationService teamCreationService) : IConsumer<Preferences>
{
    private readonly HttpClient _client = new();

    private static readonly object ClassLock = new();

    public async Task Consume(ConsumeContext<Preferences> context)
    {
        lock (ClassLock)
        {
            Console.WriteLine("Hackathon: " + context.Message.Id + " Dev: " + context.Message.Developer);
            teamCreationService.AddPreferences(context.Message);

            Console.WriteLine("team is ready? " + teamCreationService.IsTeamReady(context.Message.Id));
            if (teamCreationService.IsTeamReady(context.Message.Id))
            {
                Console.WriteLine("Team Ready");
                var teams = teamCreationService.BuildTeams(context.Message.Id);
                Console.WriteLine("Team size: " + teams.Count);
                NotifyDirectorAsync(new CreatedTeams(context.Message.Id, teams));
            }
        }
    }


    private async Task NotifyDirectorAsync(CreatedTeams request)
    {
        Console.WriteLine("Notify Director");

        var payload = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8,
            "application/json");

        while (true)
        {
            try
            {
                var response =
                    await _client.PostAsync($"http://{directorSettings.Uri}/teams",
                        payload);
                if (response.StatusCode == HttpStatusCode.OK) break;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error notifying director");
                Console.WriteLine(ex.StackTrace);
                await Task.Delay(1000);
            }
        }
    }
}