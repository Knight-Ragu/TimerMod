using System.IO;
using HarmonyLib;
using Il2Cpp;
using Il2CppQuantum;
using Il2CppQuantum_Game;
using Il2CppList = Il2CppSystem.Collections.Generic.List<Il2CppQuantum.EntityRef>;
using Il2CppPhoton.Deterministic;
using Il2CppQuantum.Physics3D;

namespace TimerMod;

public partial class Timer
{
    internal static Transform3D SpawnPosition = Transform3D.Create(FPVector3.Zero);
}

[HarmonyPatch(typeof(BikeRespawnSystem), nameof(BikeRespawnSystem.SpawnBike))]
class BikeRespawnSystem_SpawnBike_Patch
{
    public static void Prefix(Frame f, EntityRef playerEntity, ref Transform3D spawnTransform)
    {   
        if (Timer.SpawnPosition.Position != FPVector3.Zero)
        {
            spawnTransform = Timer.SpawnPosition;
            Timer.SpawnPosition.Position = FPVector3.Zero;
        }

        var e = f.Get<Player>(playerEntity).controlledEntity;
        if (f.Exists(e))
        {
            e = f.Get<Humanoid>(e).vehicle;

            if (f.Exists(e))
                Timer.CurrentRace.BikeModel = f.Get<HoverBike>(e).model;
        }
    }
}

[HarmonyPatch(typeof(RaceGameModeSystem), nameof(RaceGameModeSystem.ChangeMode))]
class RaceGameStateExtensions_RaceGameStateExtensions_Patch
{
    public unsafe static void Postfix(Frame f, MapConfig mapConfig, RaceGameState* gameState)
    {
        // f.PhysicsSceneSettings->Gravity = FPVector3.Down * FP._10;

        if (Timer.CurrentRace is not RaceInfo race) return;

        race.crossedFinishLine = false;

        if (Timer.LabelManager.TryGetLabel(0, out var l0))
            l0.PlayAnimation(LabelAnimation.None, LabelColor.White);

        Il2CppList eRefs = new();
        f.GetAllEntityRefs(eRefs);

        CheckSeedValid(mapConfig, gameState);
        RemoveSystems(f);
        DestroyPathBlockers(f, eRefs);
        SingleSegmentModeLogic(f, mapConfig, gameState);

        if (gameState->currArenaType == ArenaType.RaceStart)
        {
            if (Timer.Segment is null)
                Timer.LabelManager.Initialize(1);
            else
                Timer.LabelManager.Initialize(0);
        }
    }

    private unsafe static void SingleSegmentModeLogic(Frame f, MapConfig mapConfig, RaceGameState* gameState)
    {
        if (gameState->currArenaType == ArenaType.RaceStart)
        {
            Timer.Segment = null;

            if (ReadWrite.ReadArenaIndex(out int arenaIndex))
            {
                Timer.Segment = SingleSegment.Create(mapConfig, arenaIndex);
                if (Timer.Segment.IsStartingLine()) return;

                Arena arena = Timer.Segment.Arena();
                FPVector3 a_little_back = arena.startLinePos + -arena.startLineDirection.Normalized * (FP._3 + FP._0_50);

                if (Raycasts.StaticTerrainLineCast(f, a_little_back, a_little_back + FPVector3.Down * FP._100, out Hit3D rayHit))
                    Timer.SpawnPosition.Position = rayHit.Point + rayHit.Normal * FP._0_20;
                else
                    Timer.SpawnPosition.Position = a_little_back;
                
                Timer.SpawnPosition.Rotation = FPQuaternion.LookRotation(arena.startLineDirection);
            }
        }
    }

    private unsafe static void CheckSeedValid(MapConfig mapConfig, RaceGameState* gameState)
    {
        if (Timer.RetryInfo is not Retry retry) return;

        if (
            retry.Type != RetryMethod.InfiniteRandomSeed
         && retry.Type != RetryMethod.RandomSetQuickstopsSeed
        ) return;

        // Check to see if quickstops are going to happen in the current map

        bool quickstopsCorrect = true;

        for (int i = 0; i < mapConfig.arenas.Length; i++)
        {
            var quickstopToggle = gameState->quickStopArenaToggles.GetPointer(i)->Value;
            Timer .Log.Msg($"msg1");
            Arena arena = mapConfig.arenas[i];
            Timer .Log.Msg($"msg2");
            if (
                retry.QuickstopToggles[i] == Quickstop.Any
             || arena.arenaType != ArenaType.QuickStop
             || arena.quickStopChance == FP._1
            ) 
                continue;
            
            Timer .Log.Msg($"msg3");
            
            Timer.Log.Msg($"{quickstopToggle}: {retry.QuickstopToggles[i]}");

            Timer .Log.Msg($"msg4");

            if (quickstopToggle != (int)retry.QuickstopToggles[i])
            {
                Timer .Log.Msg($"msg5");
                quickstopsCorrect = false;
                break;
            }
        }

        if (quickstopsCorrect)
        {
            Timer.Log.Msg($"{retry.Seed} - Valid!");

            if (retry.Type == RetryMethod.InfiniteRandomSeed)
            {
                // File.WriteAllLines($"{Timer.SeedsFolder}\\{retry.Seed}", []);
                PhotonController.instance.LeaveRoom();

                return;
            }

            Timer.RetryInfo = null;
        }
        else
        {
            // Go to Main Menu and Permeate the cycle
            Timer.Log.Msg($"{retry.Seed} - Invalid,");

            PhotonController.instance.LeaveRoom();
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

    private static void DestroyPathBlockers(Frame f, Il2CppList eRefs)
    {
        if (!Timer.enabled) return;

        foreach (var entity in eRefs)
            if (f.Has<PathBlocker>(entity))
                f.Destroy(entity);
            
            // if (f.Has<HoverBike>(entity))
            // {
            //     var bike = f.Get<HoverBike>(entity);
            //     Timer.Log.Msg($"Malfunc: {bike.malfunctions}, {bike.malfunctionsSeed}");
            // }
    }
}
