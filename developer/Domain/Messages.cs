using contracts;

namespace developer.Domain;

public record AllDevs(List<Developer> Junior, List<Developer> Teamlead);

public class UserRecord
{
    public int Id { get; set; }
    public string Name { get; set; }
}
