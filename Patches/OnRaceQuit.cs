using System.IO;
using HarmonyLib;
using Il2CppQuantum;

namespace TimerMod;

[HarmonyPatch(typeof(SessionRunner), nameof(SessionRunner.Shutdown))]
class SessionRunner_Shutdown_Patch
{
    public static void Postfix() // Shutting down runner
    {
        RaceGameStateExtensions_RaceGameStateExtensions_Patch.once = true;
        if (Timer.RetryInfo is not null) return;
        if (!Timer.enabled || Timer.sprintTime == 0) return;

        Timer.Log.Msg($"Game time: {System.TimeSpan.FromSeconds(Timer.TotalSeconds):mm\\:ss\\.ff}");

        string playtimeFile = Timer.DataFolder + $"\\Playtime.txt";


        if (!Directory.Exists(Timer.DataFolder))
            Directory.CreateDirectory(Timer.DataFolder);

        if (!File.Exists(playtimeFile))
            ReadWrite.CreateNewPlaytimesFile(playtimeFile);

        ReadWrite.SavePlaytime(playtimeFile, Timer.bikeModel);


        Timer.Reset();
    }
}
