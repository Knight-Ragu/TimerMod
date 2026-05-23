using HarmonyLib;
using Il2CppQuantum;
using Il2CppQuantum_Game;

namespace TimerMod;

[HarmonyPatch(typeof(RaceGameModeSystem), nameof(RaceGameModeSystem.CrossedFinishLine))]
class RaceGameModeSystem_CrossedFinishLine_Patch
{
    public unsafe static void Prefix(Frame f, EntityRef playerEntity) // crossed the finish line
    {
        if (Timer.CurrentRace is not RaceInfo race) return;

        if (!race.crossedFinishLine)
        {
            race.crossedFinishLine = true;

            RaceGameState* gameState = f.GetOrAddSingletonPointer<RaceGameState>();

            race.SprintTimes.Add(gameState->timeInCurrentMode);


            HoverbikeModel? model = null;

            EntityRef e = f.Get<Player>(playerEntity).controlledEntity;
            if (f.Exists(e))
            {
                e = f.Get<Humanoid>(e).vehicle;

                if (f.Exists(e))
                    model = f.Get<HoverBike>(e).model;
            }
            
            string splitsFile = ReadWrite.GetSingleSegmentFilePath(f.Map.Scene, model ?? race.BikeModel);
            var wroteToFile = ReadWrite.SaveSingleSegmentTimeIfFaster(splitsFile, Timer.Segment, gameState);
            // raceTime: Timer.Segment is null ? race.RaceSumTime() : long.MaxValue
            
            if (wroteToFile && Timer.LabelManager.TryGetLabel(0, out var l0))
                l0.PlayAnimation(LabelAnimation.SlowPulse, LabelColor.Green);

            // if (wroteToSum && Timer.LabelManager.TryGetLabel(1, out var l1))
            //     l1.PlayAnimation(LabelAnimation.SlowPulse, LabelColor.Green);
        }
        else Timer.Log.Msg("Already Crossed Finish Line!");
    }
}