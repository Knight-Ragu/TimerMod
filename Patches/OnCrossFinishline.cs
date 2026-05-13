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
        if (Timer.currentRace is not RaceInfo race) return;

        if (!race.crossedFinishLine)
        {
            race.crossedFinishLine = true;

            var gameState = f.GetOrAddSingletonPointer<RaceGameState>();

            race.SprintTimes.Add(gameState->timeInCurrentMode);


            HoverbikeModel? model = null;

            var e = f.Get<Player>(playerEntity).controlledEntity;
            if (f.Exists(e))
            {
                e = f.Get<Humanoid>(e).vehicle;

                if (f.Exists(e))
                    model = f.Get<HoverBike>(e).model;
            }
            

            (string directory, string splitsFile) = ReadWrite.GetSplitsFile(SceneManager.GetActiveScene(), model);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            if (!File.Exists(splitsFile))
                ReadWrite.CreateNewSplitsFile(splitsFile);
            
            var (wroteToSprint, wroteToSum) = ReadWrite.SaveSplitTime
            (
                splitsFile,
                index: gameState->lastArenaIndex,
                sprintTime: gameState->timeInCurrentMode,
                raceTime: Timer.segmentArena is null ? race.RaceSumTime() : long.MaxValue
            );
            
            Timer.wasFastestSprint = wroteToSprint;
            Timer.wasFastestRaceSum = wroteToSum;
        }
        else Timer.Log.Msg("Already Crossed Finish Line!");
    }
}