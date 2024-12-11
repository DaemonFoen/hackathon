using contracts;
using developer;
using developer.Entities;
using Type = contracts.Type;

namespace test;

public class MemberTest
{
    private const int EmployeeCount = 5;

    private static List<Developer> GenerateEmployees(Type type, int count)
    {
        return Enumerable.Range(0, count)
            .Select(i => new Developer(i, type))
            .ToList();
    }

    private static AllDevs CreateAllMembers(out List<Developer> juniors,
        out List<Developer> teamleads)
    {
        juniors = GenerateEmployees(Type.Junior, EmployeeCount);
        teamleads = GenerateEmployees(Type.TeamLead, EmployeeCount);
        return new AllDevs(juniors, teamleads);
    }

    [Fact(DisplayName = "Заранее определённый сотрудник должен присутствовать в списке")]
    public void CorrectIdsTest()
    {
        var allMembers = CreateAllMembers(out var juniors, out var teamleads);
        var generator = new PreferencesGenerator(juniors[0], allMembers);
        var expected = teamleads.Select(it => it.Id).OrderBy(id => id).ToList();

        var preferences = generator.GenerateRandomSortedPreferences();

        Assert.Equal(expected, preferences.OrderBy(id => id).ToList());
    }

    [Fact(DisplayName = "Размер списка должен совпадать с количеством тимлидов/джунов")]
    public void EmployeeContainsTest()
    {
        var allMembers = CreateAllMembers(out var juniors, out var teamleads);
        var generator = new PreferencesGenerator(juniors[0], allMembers);

        var preferences = generator.GenerateRandomSortedPreferences();

        Assert.Equal(juniors.Count, preferences.Count);
        Assert.Equal(teamleads.Count, preferences.Count);
    }
}