using System.IO;
using HarmonyLib;
using Il2CppQuantum;

namespace TimerMod;

[HarmonyPatch(typeof(SessionRunner), nameof(SessionRunner.Shutdown))]
class SessionRunner_Shutdown_Patch
{
    public static void Postfix() // Shutting down runner
    {
        if (!Timer.enabled || Timer.Now == 0.0) return;

        Timer.Log.Msg($"Game time: {System.TimeSpan.FromSeconds(Timer.Now):mm\\:ss\\.ff}");

        string playtimeFile = Timer.DataFolder + $"\\Playtime.txt";

        Timer.Log.Msg(playtimeFile);


        if (!Directory.Exists(Timer.DataFolder))
            Directory.CreateDirectory(Timer.DataFolder);

        if (!File.Exists(playtimeFile))
            Timer.CreateNewPlaytimesFile(playtimeFile);

        Timer.SavePlaytime(playtimeFile, Timer.bikeModel);


        Timer.Reset();
    }
}
