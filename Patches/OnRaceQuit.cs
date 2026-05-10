using System.IO;
using HarmonyLib;
using Il2CppQuantum;

namespace TimerMod;

[HarmonyPatch(typeof(SessionRunner), nameof(SessionRunner.Shutdown))]
class SessionRunner_Shutdown_Patch
{
    public static void Postfix() // Shutting down runner
    {
        if (Timer.RetryInfo is not null) return;
        if (!Timer.enabled || Timer.Now == 0.0) return;

        Timer.Log.Msg($"Game time: {System.TimeSpan.FromSeconds(Timer.Now):mm\\:ss\\.ff}");

        string playtimeFile = Timer.DataFolder + $"\\Playtime.txt";


        if (!Directory.Exists(Timer.DataFolder))
            Directory.CreateDirectory(Timer.DataFolder);

        if (!File.Exists(playtimeFile))
            ReadWrite.CreateNewPlaytimesFile(playtimeFile);

        ReadWrite.SavePlaytime(playtimeFile, Timer.bikeModel);


        Timer.Reset();
    }
}
