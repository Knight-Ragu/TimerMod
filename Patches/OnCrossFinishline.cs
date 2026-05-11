using System.IO;
using HarmonyLib;
using Il2CppQuantum;
using Il2CppQuantum_Game;
using UnityEngine.SceneManagement;

namespace TimerMod;

[HarmonyPatch(typeof(RaceGameModeSystem), nameof(RaceGameModeSystem.CrossedFinishLine))]
class RaceGameModeSystem_CrossedFinishLine_Patch
{
    public unsafe static void Prefix(Frame f, EntityRef playerEntity) // crossed the finish line
    {
        if (!Timer.crossedFinishLine)
        {
            Timer.crossedFinishLine = true;
            var gameState = f.GetOrAddSingletonPointer<RaceGameState>();
            HoverbikeModel? model = null;

            var e = f.Get<Player>(playerEntity).controlledEntity;

            if (f.Exists(e))
            {
                e = f.Get<Humanoid>(e).vehicle;

                if (f.Exists(e))
                    model = f.Get<HoverBike>(e).model;
            }

            
            // gameState->timeInCurrentMode = gameState->ArenaDurationInTicks((Frame)f) - 5;

            if (gameState->playersNotYetReachedArena != 0)
                Timer.SprintTimes.Add(Timer.sprintTime);
            

            (string directory, string splitsFile) = ReadWrite.GetSplitsFile(SceneManager.GetActiveScene(), model);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            if (!File.Exists(splitsFile))
                ReadWrite.CreateNewSplitsFile(splitsFile);
            
            // (double sprintStart, double sprintEnd) = Timer.SprintTimes[^1];
            var (wroteToSprint, wroteToSum) = ReadWrite.SaveSplitTime(splitsFile, gameState->lastArenaIndex, gameState->timeInCurrentMode, Timer.RaceSumTime());
            
            Timer.wasFastestSprint = wroteToSprint;
            Timer.wasFastestRaceSum = wroteToSum;
        }
        else Timer.Log.Msg("Already Crossed Finish Line!");
    }
}