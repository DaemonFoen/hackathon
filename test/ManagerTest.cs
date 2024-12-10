using contracts;
using hrmanager;
using Type = contracts.Type;

namespace test;

public class ManagerTest
{
    private static List<Developer> PrepareEmployees(int juniorCount, int teamLeadCount)
    {
        var employees = new List<Developer>();

        for (var i = 0; i < juniorCount; i++)
        {
            employees.Add(new Developer(i, Type.Junior));
        }

        for (var i = 0; i < teamLeadCount; i++)
        {
            employees.Add(new Developer(i, Type.TeamLead));
        }

        return employees;
    }

    [Fact(DisplayName = "Количество команд должно совпадать с заранее заданным")]
    public void ShouldMatchExpectedTeamCount()
    {
        // PREPARE
        const int hackathonId = 1;
        var service = new TeamCreationService();
        var employees = PrepareEmployees(5, 5);
        foreach (var employee in employees)
        {
            var preferences = new Preferences(hackathonId, employee, [5, 4, 3, 1, 2]);
            service.AddPreferences(preferences);
        }

        // ACTION
        var teams = service.BuildTeams(hackathonId);

        // ASSERT
        Assert.Equal(employees.Count / 2, teams.Count);
    }

    [Fact(DisplayName =
        "Стратегия HRManager-а – на заранее определённых предпочтениях, должна возвращать одинаковое распределение")]
    public void ShouldReturnSameTeamDistribution()
    {
        const int hackathonId = 1;
        var teamsList = new List<List<Team>>();

        for (var j = 0; j < 3; j++)
        {
            // PREPARE
            var service = new TeamCreationService();
            var employees = PrepareEmployees(5, 5);
            foreach (var employee in employees)
            {
                var preferences = new Preferences(hackathonId, employee, [5, 4, 3, 1, 2]);
                service.AddPreferences(preferences);
            }

            // ACTION
            var teams = service.BuildTeams(hackathonId);
            teamsList.Add(teams);
        }

        // ASSERT
        Assert.All(teamsList, team => team.Equals(teamsList[0]));
    }

    [Fact(DisplayName = "Стратегия HRManager-а должна быть вызвана ровно один раз")]
    public void ShouldThrowOnSecondTeamBuildAttempt()
    {
        // PREPARE
        const int hackathonId = 1;
        var service = new TeamCreationService();
        var employees = PrepareEmployees(5, 5);
        foreach (var employee in employees)
        {
            var preferences = new Preferences(hackathonId, employee, [5, 4, 3, 1, 2]);
            service.AddPreferences(preferences);
        }

        // ACTION
        var teams = service.BuildTeams(hackathonId);

        // ASSERT
        Assert.NotEmpty(teams);
        Assert.Throws<InvalidOperationException>(() => service.BuildTeams(hackathonId));
    }
}