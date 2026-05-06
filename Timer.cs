using System.Collections.Generic;
using MelonLoader;
using UnityEngine;
using MelonLoader.Utils;

[assembly: MelonInfo(typeof(TimerMod.Timer), "TimerMod", "0.0.1", "Knight-Ragu", null)]
[assembly: MelonGame("Videocult", "Airframe")]

namespace TimerMod;

public partial class Timer : MelonMod
{
    internal static MelonLogger.Instance Log => Melon<Timer>.Instance.LoggerInstance;

    internal static string DataFolder => MelonEnvironment.UserDataDirectory + "\\TimerMod";
    internal static string TimesFolder => DataFolder + "\\Times";
    internal static string SeedFile => DataFolder + "\\Seed.txt";


    internal static bool enabled = true;

    internal static double Now = 0.0;
    internal static double? RaceStart = null;
    internal static bool wasFastestSprint = false;
    internal static bool wasFastestRaceSum = false;

    internal static List<(double start, double end)> SprintTimes = [];

    internal static double SumRaceTime()
    {
        double sum = 0.0;

        foreach (var (start, end) in SprintTimes)
            sum += end - start;

        return sum;
    }

    internal static Color TextColor(bool fast)
    {
        if (fast)
        {
            Color baseCol = Timer.TextBaseColor;
            float sin = Mathf.Sin((float)Now * 3.6f) * 0.375f + 0.62f;

            return new Color(baseCol.r * sin, baseCol.g + (1.0f - sin) * 0.25f, baseCol.b * sin);
        }
        else
            return Timer.TextBaseColor;
    }
}
