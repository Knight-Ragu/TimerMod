using System.IO;
using HarmonyLib;
using Il2CppQuantum_Game;
using UnityEngine.SceneManagement;

namespace TimerMod;

[HarmonyPatch(typeof(RaceGameModeSystem), nameof(RaceGameModeSystem.CrossedFinishLine))]
class RaceGameModeSystem_CrossedFinishLine_Patch
{
    public static void Postfix() // crossed the finish line
    {
        if (Timer.SprintStart is double t)
        {
            Timer.SprintTimes.Add((t, Timer.Now));

            // Timer.Log.Msg($"Race time: {System.TimeSpan.FromSeconds(Timer.Now - t):mm\\:ss\\.ff}, Total game time: {System.TimeSpan.FromSeconds(Timer.Now):mm\\:ss\\.ff}");
        }
        else Timer.Log.Error("Already completed race");
        

        (string directory, string splitsFile) = ReadWrite.GetSplitsFile(SceneManager.GetActiveScene(), Timer.bikeModel);

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        if (!File.Exists(splitsFile))
            ReadWrite.CreateNewSplitsFile(splitsFile);
        
        (double sprintStart, double sprintEnd) = Timer.SprintTimes[^1];
        var (wroteToSprint, wroteToSum) = ReadWrite.SaveSplitTime(splitsFile, Timer.SprintTimes.Count - 1, sprintEnd - sprintStart, Timer.SumRaceTime());
        
        Timer.wasFastestSprint = wroteToSprint;
        Timer.wasFastestRaceSum = wroteToSum;

        Timer.SprintStart = null;
    }
}