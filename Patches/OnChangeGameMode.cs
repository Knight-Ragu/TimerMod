using System.IO;
using HarmonyLib;
using Il2Cpp;
using Il2CppQuantum;
using Il2CppQuantum_Game;
using Il2CppList = Il2CppSystem.Collections.Generic.List<Il2CppQuantum.EntityRef>;
using HoverBikeModel = Il2CppQuantum.HoverbikeModel;

namespace TimerMod;

public partial class Timer
{
    internal static HoverBikeModel? bikeModel = null;
}

[HarmonyPatch(typeof(RaceGameModeSystem), nameof(RaceGameModeSystem.ChangeMode))]
class RaceGameStateExtensions_RaceGameStateExtensions_Patch
{
    internal static bool once = true;
    public unsafe static void Postfix(Frame f, RaceGameState* gameState)
    {
        Timer.Log.Msg($"{Timer.crossedFinishLine}, {gameState->currArenaType}, {gameState->currArenaIndex}");

        gameState->currArenaType = ArenaType.RaceStart;

        // if (Timer.crossedFinishLine)
        // {
        //     var toggle = gameState->quickStopArenaToggles.GetPointer(gameState->currArenaIndex);
            
        //     toggle->Value = toggle->Value == 0 ? 1 : 0;
        // }

        Timer.crossedFinishLine = false;
        Timer.wasFastestSprint = false;

        Il2CppList refs = new();
        f.GetAllEntityRefs(refs);
        
        CheckSeedValid(gameState);
        RemoveSystems(f);
        RemovePathBlockers(f, refs);
        GrabBikeModel(f, refs);
    }

    private unsafe static void CheckSeedValid(RaceGameState* gameState)
    {
        if (Timer.RetryInfo is not Retry retry) return;

        if (
            retry.Type != RetryMethod.InfiniteRandomSeed
         && retry.Type != RetryMethod.RandomQuickstopSeed
        ) return;

        // Check to see if quickstops are going to happen in the current map

        bool quickstopsCorrect = true;

        for (int i = 0; i < gameState->quickStopArenaToggles.Length; i++)
        {
            Timer.Log.Msg($"{gameState->quickStopArenaToggles.GetPointer(i)->Value}: {retry.QuickstopToggles[i]}");

            if (retry.QuickstopToggles[i] == Quickstop.Ignore) continue;

            if (gameState->quickStopArenaToggles.GetPointer(i)->Value != (int)retry.QuickstopToggles[i])
            {
                quickstopsCorrect = false;
                break;
            }
        }

        if (quickstopsCorrect)
        {
            if (retry.Type != RetryMethod.InfiniteRandomSeed)
                File.WriteAllLines($"{Timer.SeedsFolder}\\{retry.Seed}", []);

            Timer.RetryInfo = null;

            Timer.Log.Msg($"{retry.Seed} - Valid!");
        }
        else
        {
            // Go to Main Menu and Permeate the cycle
            PhotonController.instance.LeaveRoom();

            Timer.Log.Msg($"{retry.Seed} - Invalid,");
        }

        // Convert.ToString(256, 2)
    }

    private static void RemoveSystems(Frame f)
    {
        foreach(var sys in f.SystemsAll)
        {
            var type = sys.GetIl2CppType();

            if (
                type.Name == "PoliceCoordinatorSystem"
                || type.Name == "PoliceHelicopterSystem"
                || type.Name == "PoliceGunmanSystem"

                || type.Name == "PickupSpawnSystem"
                || type.Name == "SpecialPickupSpawnSystem"
            ) {
                if (Timer.enabled)
                {
                    f.SystemDisable(sys);
                    // Log.Msg("Disabled " + type.Name + " | ");
                }
                else
                {
                    f.SystemEnable(type);
                    // Log.Msg("Enabled " + type.Name + " | ");
                }
            }
        }
    }

    private static void RemovePathBlockers(Frame f, Il2CppList refs)
    {
        if (!Timer.enabled) return;

        foreach (var entity in refs)
        {
            if (f.Has<PathBlocker>(entity))
                f.Destroy(entity);
            
            // if (f.Has<HoverBike>(entity))
            // {
            //     var bike = f.Get<HoverBike>(entity);
            //     Timer.Log.Msg($"Malfunc: {bike.malfunctions}, {bike.malfunctionsSeed}");
            // }
        }
    }

    private static void GrabBikeModel(Frame f, Il2CppList refs)
    {
        foreach (var entity in refs)
        {
            if (f.Has<HoverBike>(entity))
            {
                var bike = f.Get<HoverBike>(entity);
                // Timer.Log.Msg($"Malfunc: {bike.malfunctions}, {bike.malfunctionsSeed}");
                Timer.bikeModel = bike.model;
            }
        }
    }
}