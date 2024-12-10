using contracts;
using Type = contracts.Type;

namespace developer.Domain;

public class PreferencesGenerator(Developer developer, AllDevs allDevs)
{
    public List<int> GenerateRandomSortedPreferences()
    {
        var random = new Random();
        return developer.Type == Type.Junior
            ? allDevs.Teamlead.Select(it => it.Id).OrderBy(_ => random.Next()).ToList()
            : allDevs.Junior.Select(it => it.Id).OrderBy(_ => random.Next()).ToList();
    }
}