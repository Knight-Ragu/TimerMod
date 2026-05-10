using System.Collections.Generic;
using MelonLoader;
using UnityEngine;
using MelonLoader.Utils;
using Il2Cpp;
using System.IO;

[assembly: MelonInfo(typeof(TimerMod.Timer), "TimerMod", "0.0.1", "Knight-Ragu", null)]
[assembly: MelonGame("Videocult", "Airframe")]

namespace TimerMod;

public partial class Timer : MelonMod
{
    internal static MelonLogger.Instance Log => Melon<Timer>.Instance.LoggerInstance;

    internal static string DataFolder => MelonEnvironment.UserDataDirectory + "\\TimerMod";

    internal static string RaceDataFolder => DataFolder + "\\RaceHistory";
    internal static string TimesFolder => DataFolder + "\\Times";
    internal static string SeedsFolder => DataFolder + "\\ValidSeeds";

    internal static string SeedFile => DataFolder + "\\Seed.txt";
    internal static string QuickstopsFile => DataFolder + "\\Quickstops.txt";


    internal static bool enabled = true;

    internal static double Now = 0.0;
    internal static double? SprintStart = null;
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

    internal static void Reset()
    {
        Timer.RaceText = null;
        Timer.SumText = null;

        Timer.SprintStart = null;
        Timer.bikeModel = null;
        Timer.SprintTimes.Clear();

        Timer.wasFastestSprint = false;
        Timer.wasFastestRaceSum = false;
        Timer.Now = 0.0;
    }

    public static void Retry(RetryMethod type, Quickstop[] quickstopToggles = default)
    {
        if (quickstopToggles != default)
            Timer.RetryInfo = new Retry {
                Type = type,
                QuickstopToggles = quickstopToggles,
            };
        else
            Timer.RetryInfo = new Retry {
                Type = type,
            };

        Timer.Reset();
        PhotonController.instance.LeaveRoom();
    }

    public override void OnUpdate()
    {
        var kbd = UnityEngine.InputSystem.Keyboard.current;

        // Keyboard shortcuts

        if ( // Retry map with quickstop constrained random seed
            Timer.LastConfig is not null
            && kbd.ctrlKey.isPressed
            && kbd.shiftKey.isPressed
            && kbd.rKey.wasPressedThisFrame
        ) {
            if (!File.Exists(Timer.QuickstopsFile))
                ReadWrite.CreateNewQuickstopsFile(Timer.QuickstopsFile);

            Timer.Retry(RetryMethod.RandomQuickstopSeed, ReadWrite.ReadQuickstopsFile());
            return;
        }

        if ( // Retry map with current seed
            Timer.LastConfig is not null
            && kbd.ctrlKey.isPressed
            && kbd.rKey.wasPressedThisFrame
        ) {
            Timer.Retry(RetryMethod.SameSeed);
            return;
        }

        if ( // Initiate infinite seed testing
            Timer.LastConfig is not null
            && kbd.ctrlKey.isPressed
            && kbd.shiftKey.isPressed
            && kbd.altKey.isPressed
            && kbd.iKey.wasPressedThisFrame
        ) {
            if (!Directory.Exists(Timer.SeedsFolder))
                Directory.CreateDirectory(Timer.SeedsFolder);

            Timer.Retry(RetryMethod.InfiniteRandomSeed);
            return;
        }
    }
}
