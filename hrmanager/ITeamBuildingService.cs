using contracts;

namespace hrmanager;

public interface ITeamBuildingService
{
    public List<Team> CreateTeams(List<Preferences> preferences);
}