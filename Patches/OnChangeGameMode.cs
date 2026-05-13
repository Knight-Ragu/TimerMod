using System.IO;
using HarmonyLib;
using Il2Cpp;
using Il2CppQuantum;
using Il2CppQuantum_Game;
using Il2CppList = Il2CppSystem.Collections.Generic.List<Il2CppQuantum.EntityRef>;
using System.Linq;
using Il2CppPhoton.Deterministic;
using Il2CppQuantum.Physics3D;

namespace TimerMod;

[HarmonyPatch(typeof(RaceGameModeSystem), nameof(RaceGameModeSystem.ChangeMode))]
class RaceGameStateExtensions_RaceGameStateExtensions_Patch
{
    public unsafe static void Postfix(Frame f, MapConfig mapConfig, RaceGameState* gameState)
    {
        if (Timer.currentRace is not RaceInfo race) return;

        race.crossedFinishLine = false;
        Timer.wasFastestSprint = false;

        Il2CppList refs = new();
        f.GetAllEntityRefs(refs);

        CheckSeedValid(mapConfig, gameState);
        RemoveSystems(f);
        DestroyPathBlockers(f, refs);
        SetSegmentModeLogic(f, mapConfig, gameState, refs);
    }

    private unsafe static void SetSegmentModeLogic(Frame f, MapConfig mapConfig, RaceGameState* gameState, Il2CppList refs)
    {
        if (gameState->currArenaType == ArenaType.RaceStart)
        {
            Timer.segmentArena = null;
            Timer.SpawnPosition.Position = FPVector3.Zero;

            if (ReadWrite.ReadArenaIndex(out int arenaIndex))
            {
                Timer.segmentArena = SingleSegment.Create(mapConfig, arenaIndex);
                if (Timer.segmentArena.IsStartingLine()) return;

                Arena arena = Timer.segmentArena.GetArena();

                Timer.SpawnPosition.Position = arena.startLinePos;
                Timer.SpawnPosition.Rotation = FPQuaternion.LookRotation(arena.startLineDirection, FPVector3.Up);
                
                var a_little_back = Timer.SpawnPosition.Position + Timer.SpawnPosition.Back * (FP._3 + FP._0_50);

                if (Raycasts.StaticTerrainLineCast(f, a_little_back, a_little_back + FPVector3.Down * FP._100, out Hit3D rayHit))
                {
                    Timer.SpawnPosition.Position = rayHit.Point + rayHit.Normal * FP._0_20;
                }
                else
                {
                    Timer.SpawnPosition.Position = a_little_back;
                }
            }
        }
    }

    private unsafe static void CheckSeedValid(MapConfig mapConfig, RaceGameState* gameState)
    {
        if (Timer.RetryInfo is not Retry retry) return;

        if (
            retry.Type != RetryMethod.InfiniteRandomSeed
         && retry.Type != RetryMethod.RandomQuickstopSeed
        ) return;

        // If all arenas are not quickstops / guaranteed quickstops, abort
        if (mapConfig.arenas.All((a) => a.arenaType != ArenaType.QuickStop || a.quickStopChance == FP._1)) {
            Timer.RetryInfo = null;
            return;
        }

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

    private static void DestroyPathBlockers(Frame f, Il2CppList refs)
    {
        if (!Timer.enabled) return;

        foreach (var entity in refs)
            if (f.Has<PathBlocker>(entity))
                f.Destroy(entity);
            
            // if (f.Has<HoverBike>(entity))
            // {
            //     var bike = f.Get<HoverBike>(entity);
            //     Timer.Log.Msg($"Malfunc: {bike.malfunctions}, {bike.malfunctionsSeed}");
            // }
    }
}
