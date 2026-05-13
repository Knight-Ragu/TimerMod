namespace TimerMod;

public class Retry()
{
    public RetryMethod Type = RetryMethod.SameSeed;
    public Quickstop[] QuickstopToggles = [
        Quickstop.Ignore,
        Quickstop.Ignore,
        Quickstop.Enable,
        Quickstop.Ignore,
        Quickstop.Enable,
        Quickstop.Ignore,
        Quickstop.Ignore,
        Quickstop.Ignore
    ];

    internal int Seed = 0;
}

public enum RetryMethod {
    SameSeed,
    RandomSeed,
    InfiniteRandomSeed,
    RandomQuickstopSeed,
}

public enum Quickstop {
    Enable = 1,
    Disable = 0,
    Ignore = -1,
}