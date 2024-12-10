using contracts;

namespace developer.Domain;

public abstract class WorkerState
{
    private WorkerState()
    {
    }

    public sealed class Empty : WorkerState
    {
        public static readonly Empty Instance = new();

        private Empty()
        {
        }
    }

    public sealed class Ready(Developer developer, AllDevs allDevs) : WorkerState
    {
        public Developer Developer { get; } = developer;
        public AllDevs AllDevs { get; } = allDevs;
        public PreferencesGenerator PreferencesGenerator { get; } = new(developer, allDevs);
    }
}