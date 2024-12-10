using System.ComponentModel.DataAnnotations;
using contracts;

namespace hrdirector.Entities;

public record Hackathon
{
    [Key] public int HackathonId { get; set; }
    public double Harmony { get; set; }
    public List<Team> Teams { get; set; } = []; // может быть null
    public List<Preferences> Preferences { get; set; } = []; // может быть null
}