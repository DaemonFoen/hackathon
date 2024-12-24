using System.Collections.Concurrent;
using contracts;
using Type = contracts.Type;

namespace hrmanager;

public class TeamCreationService(ITeamBuildingService teamBuildingService)
{
    private readonly ConcurrentDictionary<int, List<Preferences>> _hackathonPreferences = new();
    private const int TeamSize = 10;
    private static readonly object ClassLock = new();

    public void AddPreferences(Preferences preferences)
    {
        lock (ClassLock)
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
    }

    public bool IsTeamReady(int hackathonId)
    {
        return _hackathonPreferences.TryGetValue(hackathonId, out var preferences) &&
               preferences.Count == TeamSize;
    }

    public List<Team> BuildTeams(int hackathonId)
    {
        lock (ClassLock)
        {
            if (!_hackathonPreferences.TryGetValue(hackathonId, out var preferences))
            {
                throw new InvalidOperationException("Hackathon preferences not found.");
            }

            var teams = teamBuildingService.CreateTeams(preferences);
            RemoveHackathonPreferences(hackathonId);
            return teams;
        }
    }

    private void RemoveHackathonPreferences(int hackathonId)
    {
        _hackathonPreferences.TryRemove(hackathonId, out _);
    }
}