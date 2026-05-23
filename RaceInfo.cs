using System.Collections.Generic;
using Il2CppQuantum;

namespace TimerMod;

public class RaceInfo()
{
    public HoverbikeModel? BikeModel = null;
    public double TotalElapsedSeconds => (double)totalElapsedTime / 45.0;
    public long totalElapsedTime = 0;

    public List<long> SprintTimes = [];

    public bool crossedFinishLine = false;

    internal long RaceSumTime()
    {
        long sum = 0;

        foreach (var sprint in SprintTimes)
            sum += sprint;

        return sum;
    }

    internal double RaceSumSeconds() => (double)RaceSumTime() / 45.0;
}