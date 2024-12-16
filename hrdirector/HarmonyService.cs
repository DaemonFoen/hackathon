using System.Data.Entity.Core;
using System.Runtime.CompilerServices;
using contracts;
using hrdirector.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Type = contracts.Type;

namespace hrdirector;

public class HarmonyService(
    IDbContextFactory<ApplicationDbContext> contextFactory,
    IPublishEndpoint publishEndpoint)
{
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void AddPreferences(int hackathonId, Preferences preferences)
    {
        try
        {
            using var context = contextFactory.CreateDbContext();
            Console.WriteLine("Hackathon id: " + hackathonId + " preferences: " + preferences.Id);
            var hackathon = context.Entities.First(e => e.HackathonId == hackathonId);
            if (hackathon == null)
            {
                throw new InvalidOperationException("Could not find hackathon with id: " + hackathonId);
            }
            
            hackathon.Preferences.Add(preferences);
            Console.WriteLine("Hackathon teams: " + hackathon.Teams);
            if (hackathon.Teams.Count != 0 && hackathon.Teams.Count == hackathon.Preferences.Count)
            {
                hackathon.Harmony = CalculateHarmony(hackathon.Teams, hackathon.Preferences);
            }
            
            context.Entities.Update(hackathon);

            context.SaveChanges();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error when updating preferences for hackathon: " + hackathonId);
        }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void AddTeams(int hackathonId, List<Team> teams)
    {
        using var context = contextFactory.CreateDbContext();
        try
        {
            var hackathon = context.Entities.FirstOrDefault(e => e.HackathonId == hackathonId);
            if (hackathon == null)
            {
                throw new InvalidOperationException("hackathon is null");
            }

            hackathon.Teams.AddRange(teams);
            if (hackathon.Preferences.Count == 10 && teams.Count == hackathon.Preferences.Count)
            { 
                hackathon.Harmony = CalculateHarmony(hackathon.Teams, hackathon.Preferences);
            }
            context.Entities.Update(hackathon);

            context.SaveChanges();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error when updating team for hackathon: " + hackathonId);
        }
    }

    public static double CalculateHarmony(List<Team> teams, List<Preferences> preferences)
    {
        var max = teams.Count;

        var many = teams.SelectMany<Team, int>(team =>
            {
                var junPrefs = preferences.Find(it =>
                    it.Developer.Type == Type.Junior && it.Developer.Id == team.Junior.Id)!;
                var tlPrefs = preferences.Find(it =>
                    it.Developer.Type == Type.TeamLead && it.Developer.Id == team.TeamLead.Id)!;

                var junToTeamLeadRate = max - junPrefs.PreferencesList.IndexOf(team.TeamLead.Id);
                var teamLeadToJunRate = max - tlPrefs.PreferencesList.IndexOf(team.Junior.Id);

                return [junToTeamLeadRate, teamLeadToJunRate];
            }
        ).ToList();

        return HarmonyImpl(many);
    }

    public static double HarmonyImpl(List<int> seq)
    {
        var acc = seq.Sum(x => 1m / x);
        return (double)(seq.Count / acc);
    }

    public async Task StartHackathonsAsync(int totalHackathons, TimeSpan delay)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        for (var i = 0; i < totalHackathons; i++)
        {
            var hackathonId = new Random().Next();
            var hackathon = new Hackathon { HackathonId = hackathonId };
            context.Entities.Add(hackathon);

            Console.WriteLine("StartHackathon");
            var startEvent = new HackathonStartEvent(hackathonId);
            
            await context.SaveChangesAsync();

            await publishEndpoint.Publish(startEvent);
            await Task.Delay(delay);
        }
    }

    public IEnumerable<int> GetAllHackathonIds()
    {
        using var context = contextFactory.CreateDbContext();
        return context.Entities.AsNoTracking().Select(e => e.HackathonId).ToList();
    }

    public Hackathon? GetHackathonById(int hackathonId)
    {
        using var context = contextFactory.CreateDbContext();
        return context.Entities.FirstOrDefault(e => e.HackathonId == hackathonId);
    }

    public double? GetAverageHarmony()
    {
        using var context = contextFactory.CreateDbContext();
        var harmonies = context.Entities.Select(e => e.Harmony).ToList();
        return harmonies.Count != 0 ? harmonies.Average() : null;
    }

    public List<Team>? GetTeamsForHackathon(int hackathonId)
    {
        using var context = contextFactory.CreateDbContext();
        var hackathon = context.Entities.FirstOrDefault(e => e.HackathonId == hackathonId);

        return hackathon?.Teams;
    }

    public List<Preferences>? GetPreferencesForHackathon(int hackathonId)
    {
        using var context = contextFactory.CreateDbContext();
        var hackathon = context.Entities.FirstOrDefault(e => e.HackathonId == hackathonId);
        return hackathon?.Preferences;
    }

    public void SetHarmony(int hackathonId)
    {
        using var context = contextFactory.CreateDbContext();
        try
        {
            var hackathon = context.Entities.FirstOrDefault(e => e.HackathonId == hackathonId);

            if (hackathon == null)
            {
                throw new EntityException("Hackathon not found");
            }

            hackathon.Harmony = CalculateHarmony(hackathon.Teams, hackathon.Preferences);
            context.Entities.Update(hackathon);
            context.SaveChanges();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error when updating team for hackathon: " + hackathonId);
        }
    }
}