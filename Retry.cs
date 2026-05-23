namespace TimerMod;

public class Retry()
{
    public RetryMethod Type = RetryMethod.SameSeed;
    public Quickstop[] QuickstopToggles = [
        Quickstop.Any,
        Quickstop.Any,
        Quickstop.Enable,
        Quickstop.Any,
        Quickstop.Enable,
        Quickstop.Any,
        Quickstop.Any,
        Quickstop.Any
    ];

    internal int Seed = 0;
}

public enum RetryMethod {
    SameSeed,
    RandomSeed,
    InfiniteRandomSeed,
    RandomSetQuickstopsSeed,
}

public enum Quickstop {
    Any = -1,
    Enable = 1,
    Disable = 0,
}