using System.Text.Json;
using contracts;
using hrdirector;
using MassTransit;

namespace Hrdirector;

public class Director(
    HarmonyService harmonyService) : BackgroundService
{
    private const int TotalHackathons = 5;
    private static readonly TimeSpan HackathonStartDelay = TimeSpan.FromSeconds(10);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        InitializeHttpServer(stoppingToken);

        await harmonyService.StartHackathonsAsync(
            TotalHackathons,
            HackathonStartDelay
        );

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private void InitializeHttpServer(CancellationToken stoppingToken)
    {
        var app = WebApplication.CreateBuilder().Build();
     
        // app.MapHealthChecks("/health");
        
        app.MapPost("/teams", TeamsRequestHandler);

        app.MapGet("/hackathon", (HttpContext context) =>
        {
            if (!context.Request.Query.TryGetValue("id", out var idValue) ||
                !int.TryParse(idValue, out var hackathonId))
            {
                return Results.BadRequest(new { Message = "Parameter 'id' is required and must be an integer." });
            }

            var hackathon = harmonyService.GetHackathonById(hackathonId);
            return hackathon != null
                ? Results.Json(hackathon)
                : Results.NotFound(new { Message = $"Hackathon with ID {hackathonId} not found." });
        });

        app.MapGet("/all-hackathons", () =>
        {
            try
            {
                return Results.Json(harmonyService.GetAllHackathonIds());
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { Message = "Error retrieving hackathons", Error = ex.Message });
            }
        });

        app.MapGet("/avg-harmony", () =>
        {
            try
            {
                return Results.Json(harmonyService.GetAverageHarmony());
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { Message = "Error calculating average harmony", Error = ex.Message });
            }
        });
        
        app.RunAsync(stoppingToken);
    }

    private IResult TeamsRequestHandler(CreatedTeams request)
    {
        try
        {
            Console.WriteLine($"HackathonId: {request.HackathonId}");
            foreach (var team in request.Teams)
            {
                Console.WriteLine($"{team.TeamLead} - {team.Junior}");
            }

            harmonyService.AddTeams(request.HackathonId, request.Teams);
            Console.WriteLine($"Avg harmony: {harmonyService.GetAverageHarmony()}");
            return Results.Ok();
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { Message = "Error processing teams", Error = ex.Message });
        }
    }
}