// using System.Text;
// using System.Text.Json;
// using developer.Domain;
// using RabbitMQService;
//
// namespace developer;
//
// public class Worker(
//     ILogger<Worker> logger,
//     RabbitMqService rabbitMqService) : BackgroundService
// {
//     private readonly HttpClient _client = new();
//     private int _processedHackathonCount;
//     private WorkerState _state = WorkerState.Empty.Instance;
//
//     protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//     {
//         // await SignUpAsync(stoppingToken);
//
//         if (_state is WorkerState.Ready readyState)
//         {
//             var topic = GenerateSubscriptionTopic(readyState);
//             SubscribeToTopic(topic);
//
//             await MonitorHackathonsAsync(stoppingToken);
//         }
//         else
//         {
//             await Console.Error.WriteLineAsync("Worker is not ready. Unable to proceed.");
//         }
//     }
//
//     // private async Task SignUpAsync(CancellationToken stoppingToken)
//     // {
//     //     var uuid = Guid.NewGuid().ToString();
//     //     var request = new SignUpMePlease(uuid);
//     //
//     //     try
//     //     {
//     //         var response = await AttemptSignUpAsync(request, stoppingToken);
//     //         var developer = response.Developer;
//     //         var _allDevs = response.AllDevs;
//     //         if (developer == null || _allDevs == null) throw new ApplicationException("Unable to sign up.");
//     //
//     //         _state = new WorkerState.Ready(developer, _allDevs);
//     //         Console.WriteLine($"Sign-up successful: Developer={developer}");
//     //     }
//     //     catch (Exception e)
//     //     {
//     //         Console.Error.WriteLine(e);
//     //     }
//     // }
//
//     // private async Task<OperatorResponse> AttemptSignUpAsync(SignUpMePlease request, CancellationToken stoppingToken)
//     // {
//     //     var payload = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
//     //     using var response = await _client.PostAsync(
//     //         $"http://{operatorSettings.Uri}/sign-up-for-a-hackathon",
//     //         payload,
//     //         stoppingToken
//     //     );
//     //
//     //     response.EnsureSuccessStatusCode();
//     //
//     //     var result = await response.Content.ReadAsStringAsync(stoppingToken);
//     //     return JsonSerializer.Deserialize<OperatorResponse>(
//     //         result,
//     //         new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
//     //     ) ?? throw new JsonException("Invalid operator response.");
//     // }
//
//     private static string GenerateSubscriptionTopic(WorkerState.Ready state)
//     {
//         return $"hackathon.start.{state.Developer.Type.ToString().ToLower()}.{state.Developer.Id}";
//     }
//
//     private void SubscribeToTopic(string topic)
//     {
//         rabbitMqService.Subscribe(topic, OnMessage);
//         Console.WriteLine($"Subscribed to topic: {topic}");
//     }
//
//     private static async Task MonitorHackathonsAsync(CancellationToken stoppingToken)
//     {
//         while (!stoppingToken.IsCancellationRequested)
//         {
//             await Task.Delay(100, stoppingToken);
//         }
//     }
//
//     private void OnMessage(string message)
//     {
//         if (_state is WorkerState.Ready readyState)
//         {
//             Console.WriteLine($"{readyState.Developer.Type}[{readyState.Developer.Id}] received message: {message}");
//
//             try
//             {
//                 ProcessMessage(message, readyState);
//             }
//             catch (Exception ex)
//             {
//                 Console.Error.WriteLine(ex);
//             }
//         }
//         else
//         {
//             logger.LogWarning("Received message while Worker is not ready.");
//         }
//     }
//
//     private void ProcessMessage(string message, WorkerState.Ready state)
//     {
//         var hackathonMessage = JsonSerializer.Deserialize<HackathonStartMessage>(
//             message,
//             new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
//         ) ?? throw new JsonException("Failed to deserialize HackathonStartMessage.");
//
//         var preferences = state.PreferencesGenerator.GenerateRandomSortedPreferences();
//         var preferencesResponse = new Preferences(hackathonMessage.Id, state.Developer, preferences);
//         var serializedResponse = JsonSerializer.Serialize(preferencesResponse);
//
//         rabbitMqService.Send(serializedResponse, "hackathon.preferences");
//         _processedHackathonCount++;
//     }
// }