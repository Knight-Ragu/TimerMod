using System.Collections.Generic;
using MelonLoader;
using UnityEngine;
using MelonLoader.Utils;
using Il2Cpp;
using System.IO;
using Il2CppQuantum;

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


    internal static Retry? RetryInfo = null;
    internal static RuntimeConfig? LastConfig = null;

    internal static SingleSegment? Segment = null;
    internal static RaceInfo CurrentRace = new();
    internal static LabelManager LabelManager = new();

    internal static void Reset()
    {
        Timer.CurrentRace = new();
        Timer.Segment = null;
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
        Timer.LabelManager.Update();

        var kbd = UnityEngine.InputSystem.Keyboard.current;

        // Keyboard shortcuts

        if (kbd.gKey.wasPressedThisFrame && Timer.LabelManager.TryGetLabel(0, out var l0))
            l0.PlayAnimation(LabelAnimation.SlowPulse, LabelColor.Green);

        if (kbd.yKey.wasPressedThisFrame && Timer.LabelManager.TryGetLabel(0, out var l1))
            l1.PlayAnimation(LabelAnimation.SlowPulse, LabelColor.Gold);

        if (kbd.rKey.wasPressedThisFrame && Timer.LabelManager.TryGetLabel(0, out var l2))
            l2.PlayAnimation(LabelAnimation.QuickFade, LabelColor.Red);

        if (kbd.bKey.wasPressedThisFrame && Timer.LabelManager.TryGetLabel(0, out var l3))
            l3.PlayAnimation(LabelAnimation.SlowFade, LabelColor.Blue);

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
