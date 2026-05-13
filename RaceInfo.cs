using System.Collections.Generic;

namespace TimerMod;

public class RaceInfo()
{
    internal double TotalElapsedSeconds => (double)totalElapsedTime / 45.0;
    internal long totalElapsedTime = 0;

    internal List<long> SprintTimes = [];

    internal bool crossedFinishLine = false;

    internal long RaceSumTime()
    {
        long sum = 0;

        foreach (var sprint in SprintTimes)
            sum += sprint;

        return sum;
    }

    internal double RaceSumSeconds() => (double)RaceSumTime() / 45.0;
}