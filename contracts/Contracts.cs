// See https://aka.ms/new-console-template for more information

namespace contracts;

public record HackathonStartEvent(int HackathonId);

public record Preferences(int Id, Developer Developer, List<int> PreferencesList);

public record Developer(int Id, Type Type);

public enum Type
{
    Junior,
    TeamLead
}

public record CreatedTeams(int HackathonId, List<Team> Teams);

public record Team(Developer Junior, Developer TeamLead);


internal class Program
{
    public static void Main(string[] args)
    {
    }
}