using contracts;
using hrdirector;
using Type = contracts.Type;

namespace test;

public class DirectorTest
{
    [Fact(DisplayName = "Среднее гармоническое одинаковых чисел равно им всем")]
    public void TodoName1()
    {
        const int hackathonId = 1;
        var teams = new List<Team>
        {
            new(new Developer(1, Type.Junior), new Developer(1, Type.TeamLead)),
            new(new Developer(2, Type.Junior), new Developer(2, Type.TeamLead)),
            new(new Developer(3, Type.Junior), new Developer(3, Type.TeamLead)),
            new(new Developer(4, Type.Junior), new Developer(4, Type.TeamLead)),
            new(new Developer(5, Type.Junior), new Developer(5, Type.TeamLead))
        };
        var preferences = new List<Preferences>
        {
            new(hackathonId, new Developer(1, Type.Junior), [1, 2, 3, 4, 5]),
            new(hackathonId, new Developer(2, Type.Junior), [2, 1, 3, 4, 5]),
            new(hackathonId, new Developer(3, Type.Junior), [3, 2, 1, 4, 5]),
            new(hackathonId, new Developer(4, Type.Junior), [4, 2, 3, 1, 5]),
            new(hackathonId, new Developer(5, Type.Junior), [5, 2, 3, 4, 1]),
            new(hackathonId, new Developer(1, Type.TeamLead), [1, 2, 3, 4, 5]),
            new(hackathonId, new Developer(2, Type.TeamLead), [2, 1, 3, 4, 5]),
            new(hackathonId, new Developer(3, Type.TeamLead), [3, 2, 1, 4, 5]),
            new(hackathonId, new Developer(4, Type.TeamLead), [4, 2, 3, 1, 5]),
            new(hackathonId, new Developer(5, Type.TeamLead), [5, 2, 3, 4, 1])
        };

        var harmony = HarmonyService.CalculateHarmony(teams, preferences);

        Assert.Equal(5.0, harmony);
    }

    [Fact(DisplayName = "Среднее гармоническое одинаковых чисел равно им всем")]
    public void TodoName2()
    {
        const int hackathonId = 1;
        var teams = new List<Team>
        {
            new(new Developer(1, Type.Junior), new Developer(1, Type.TeamLead)),
        };
        var preferences = new List<Preferences>
        {
            new(hackathonId, new Developer(1, Type.Junior), [1]),
            new(hackathonId, new Developer(1, Type.TeamLead), [1]),
        };

        var harmony = HarmonyService.CalculateHarmony(teams, preferences);

        Assert.Equal(1.0, harmony);
    }

    [Theory(DisplayName = "Другие варианты среднего гармонического")]
    [InlineData(new[] { 2, 6 }, 3.0)]
    [InlineData(new[] { 1, 1 }, 1.0)]
    [InlineData(new[] { 2, 4, 6 }, 3.2727272727272729)]
    [InlineData(new[] { 3, 5, 7 }, 4.4366197183098599)]
    public void CalculateHarmonicMean(int[] numbers, double expected)
    {
        var harmony = HarmonyService.HarmonyImpl(numbers.ToList());
        Assert.True(Math.Abs(harmony - expected) < 0.01);
    }

    [Fact(DisplayName = "Определённые списки предпочтений и команды, должны дать, заранее определённое значение")]
    public void TodoName3()
    {
        const int hackathonId = 1;
        var teams = new List<Team>
        {
            new(new Developer(1, Type.Junior), new Developer(1, Type.TeamLead)),
            new(new Developer(2, Type.Junior), new Developer(2, Type.TeamLead)),
            new(new Developer(3, Type.Junior), new Developer(3, Type.TeamLead)),
            new(new Developer(4, Type.Junior), new Developer(4, Type.TeamLead)),
            new(new Developer(5, Type.Junior), new Developer(5, Type.TeamLead))
        };
        var preferences = new List<Preferences>
        {
            new(hackathonId, new Developer(5, Type.Junior), [1, 2, 3, 4, 5]),
            new(hackathonId, new Developer(4, Type.Junior), [2, 1, 3, 4, 5]),
            new(hackathonId, new Developer(3, Type.Junior), [3, 2, 1, 4, 5]),
            new(hackathonId, new Developer(2, Type.Junior), [4, 2, 3, 1, 5]),
            new(hackathonId, new Developer(1, Type.Junior), [5, 2, 3, 4, 1]),
            new(hackathonId, new Developer(5, Type.TeamLead), [1, 2, 3, 4, 5]),
            new(hackathonId, new Developer(4, Type.TeamLead), [2, 1, 3, 4, 5]),
            new(hackathonId, new Developer(3, Type.TeamLead), [3, 2, 1, 4, 5]),
            new(hackathonId, new Developer(2, Type.TeamLead), [4, 2, 3, 1, 5]),
            new(hackathonId, new Developer(1, Type.TeamLead), [5, 2, 3, 4, 1])
        };

        var harmony = HarmonyService.CalculateHarmony(teams, preferences);

        Assert.True(Math.Abs(harmony - 1.6949152542372881) < 0.01);
    }

    [Fact(DisplayName = "Определённые списки предпочтений и команды, должны дать, заранее определённое значение")]
    public void TodoName4()
    {
        const int hackathonId = 1;
        var teams = new List<Team>
        {
            new(new Developer(1, Type.Junior), new Developer(1, Type.TeamLead)),
            new(new Developer(2, Type.Junior), new Developer(2, Type.TeamLead)),
            new(new Developer(3, Type.Junior), new Developer(3, Type.TeamLead)),
            new(new Developer(4, Type.Junior), new Developer(4, Type.TeamLead)),
            new(new Developer(5, Type.Junior), new Developer(5, Type.TeamLead))
        };
        var preferences = new List<Preferences>
        {
            new(hackathonId, new Developer(1, Type.Junior), [1, 2, 3, 4, 5]),
            new(hackathonId, new Developer(2, Type.Junior), [2, 1, 3, 4, 5]),
            new(hackathonId, new Developer(3, Type.Junior), [3, 2, 1, 4, 5]),
            new(hackathonId, new Developer(4, Type.Junior), [4, 2, 3, 1, 5]),
            new(hackathonId, new Developer(5, Type.Junior), [5, 2, 3, 4, 1]),
            new(hackathonId, new Developer(5, Type.TeamLead), [1, 2, 3, 4, 5]),
            new(hackathonId, new Developer(4, Type.TeamLead), [2, 1, 3, 4, 5]),
            new(hackathonId, new Developer(3, Type.TeamLead), [3, 2, 1, 4, 5]),
            new(hackathonId, new Developer(2, Type.TeamLead), [4, 2, 3, 1, 5]),
            new(hackathonId, new Developer(1, Type.TeamLead), [5, 2, 3, 4, 1])
        };

        var harmony = HarmonyService.CalculateHarmony(teams, preferences);

        Assert.True(Math.Abs(harmony - 2.5316455696202533) < 0.01);
    }
}