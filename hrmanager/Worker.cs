// using System.Collections.Concurrent;
// using System.Net;
// using System.Text;
// using System.Text.Json;
// using RabbitMQService;
//
// namespace hrmanager;
//
// public class Worker(
//     PreferencesResponseConsumer p,
//     DirectorSettings directorSettings,
//     ILogger<Worker> logger,
//     TeamCreationService teamCreationService
// ) : BackgroundService
// {
//     private readonly HttpClient _client = new();
//     private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
//
//     protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//     {
//         rabbitMqService.Subscribe(
//             "hackathon.preferences.hr.manager",
//             message => ProcessPreferencesMessageAsync(message, stoppingToken)
//         );
//
//         while (!stoppingToken.IsCancellationRequested)
//         {
//             await Task.Delay(1000, stoppingToken);
//         }
//     }
//
//     private async void ProcessPreferencesMessageAsync(string serializedMessage, CancellationToken stoppingToken)
//     {
//         // try
//         // {
//         //     var preferences = DeserializeMessage(serializedMessage);
//         //
//         //     teamCreationService.AddPreferences(preferences);
//         //
//         //     if (!teamCreationService.IsTeamReady(preferences.Id))
//         //     {
//         //         return;
//         //     }
//         //
//         //     var teams = teamCreationService.BuildTeams(preferences.Id);
//         //     await NotifyDirectorAsync(new CreatedTeams(preferences.Id, teams), stoppingToken);
//         // }
//         // catch (Exception ex)
//         // {
//         //     logger.LogError(ex, "Error processing message");
//         //     throw;
//         // }
//     }
//
//     private Preferences DeserializeMessage(string serializedMessage)
//     {
//         return JsonSerializer.Deserialize<Preferences>(serializedMessage, _jsonOptions)
//                ?? throw new JsonException("Failed to deserialize preferences message.");
//     }
//
//     private async Task NotifyDirectorAsync(CreatedTeams request, CancellationToken stoppingToken)
//     {
//         var payload = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
//
//         while (true)
//         {
//             try
//             {
//                 var response = await _client.PostAsync($"http://{directorSettings.Uri}/teams", payload,
//                     stoppingToken);
//                 if (response.StatusCode == HttpStatusCode.OK) break;
//             }
//             catch (Exception ex)
//             {
//                 logger.LogError(ex, "Error notifying director");
//                 await Task.Delay(1000, stoppingToken);
//             }
//         }
//     }
// }