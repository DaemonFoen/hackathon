using System.Collections.Concurrent;
using contracts;
using Type = contracts.Type;

namespace hrmanager;

public class TeamCreationService
{
    private readonly ConcurrentDictionary<int, List<Preferences>> _hackathonPreferences = new();
    private const int TeamSize = 10;

    public void AddPreferences(Preferences preferences)
    {
        var hackathonId = preferences.Id;

        _hackathonPreferences.AddOrUpdate(
            hackathonId,
            _ => [preferences],
            (_, list) =>
            {
                list.Add(preferences);
                return list;
            }
        );
    }

    public bool IsTeamReady(int hackathonId)
    {
        return _hackathonPreferences.TryGetValue(hackathonId, out var preferences) && preferences.Count == TeamSize;
    }

    public List<Team> BuildTeams(int hackathonId)
    {
        if (!_hackathonPreferences.TryGetValue(hackathonId, out var preferences))
        {
            throw new InvalidOperationException("Hackathon preferences not found.");
        }

        var teams = CreateTeams(preferences);
        RemoveHackathonPreferences(hackathonId);
        return teams;
    }

    private void RemoveHackathonPreferences(int hackathonId)
    {
        _hackathonPreferences.TryRemove(hackathonId, out _);
    }

    private static List<Team> CreateTeams(List<Preferences> preferences)
    {
        var juniors = preferences.Where(p => p.Developer.Type == Type.Junior).ToList();
        var teamLeads = preferences.Where(p => p.Developer.Type == Type.TeamLead).ToList();

        var compatibility = CalculateCompatibilityMatrix(juniors, teamLeads);

        return MatchTeams(juniors, teamLeads, compatibility);
    }

    private static Dictionary<int, Dictionary<int, float>> CalculateCompatibilityMatrix(
        List<Preferences> juniors,
        List<Preferences> teamLeads)
    {
        var compatibility = new Dictionary<int, Dictionary<int, float>>();

        foreach (var junior in juniors)
        {
            compatibility[junior.Developer.Id] = teamLeads.ToDictionary(
                teamLead => teamLead.Developer.Id,
                teamLead => CalculateCompatibilityScore(junior, teamLead)
            );
        }

        return compatibility;
    }

    private static float CalculateCompatibilityScore(Preferences junior, Preferences teamLead)
    {
        var juniorPrefs = junior.PreferencesList;
        var teamLeadPrefs = teamLead.PreferencesList;
        var max = teamLead.PreferencesList.Count;

        var juniorToLead = juniorPrefs.Contains(teamLead.Developer.Id)
            ? max - juniorPrefs.IndexOf(teamLead.Developer.Id)
            : 0;

        var leadToJunior = teamLeadPrefs.Contains(junior.Developer.Id)
            ? max - teamLeadPrefs.IndexOf(junior.Developer.Id)
            : 0;

        return juniorToLead > 0 && leadToJunior > 0
            ? 1f / juniorToLead + 1f / leadToJunior
            : float.MinValue;
    }

    private static List<Team> MatchTeams(
        List<Preferences> juniors,
        List<Preferences> teamLeads,
        Dictionary<int, Dictionary<int, float>> compatibility)
    {
        var teams = new List<Team>();
        var assignedTeamLeads = new HashSet<int>();
        var assignedJuniors = new HashSet<int>();

        foreach (var junior in juniors)
        {
            var bestTeamLead = teamLeads
                .Where(tl => !assignedTeamLeads.Contains(tl.Developer.Id))
                .OrderByDescending(tl => compatibility[junior.Developer.Id][tl.Developer.Id])
                .FirstOrDefault();

            if (bestTeamLead == null)
            {
                continue;
            }

            teams.Add(new Team(junior.Developer, bestTeamLead.Developer));
            assignedTeamLeads.Add(bestTeamLead.Developer.Id);
            assignedJuniors.Add(junior.Developer.Id);
        }

        return teams;
    }
}