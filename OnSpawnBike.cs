using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using HarmonyLib;
using Il2Cpp;
using Il2CppPhoton.Deterministic;
using Il2CppQuantum;
using Il2CppQuantum.Core;
using Il2CppQuantum_Game;
using Il2CppQuantum_HoverBike;
using Il2CppSystem.Runtime.CompilerServices;
using Il2CppView_Humanoid;
using UnityEngine;

namespace TimerMod;

public partial class Timer
{
    internal static int oftenity = 50;
    internal static List<AssetRef<EntityPrototype>> prototypes = [];

    [HarmonyPatch(typeof(FrameContext), "OnFrameSimulationBegin")]
    private class Simulate
    {
        public static void Postfix(FrameBase f)
        {
            
        }
    }

    internal static SessionContainer? SeshContainer = null;
    [HarmonyPatch(typeof(PhotonController), "ChangeSceneAndStartQuantumGameCoroutine")]
    private class StartQuantumGame
    {
        // b4

        public static void Prefix(ref RuntimeConfig runtimeConfig)
        {
            Log.Msg($"RuntimeConfig: {runtimeConfig.Dump()}");
            Log.Msg($"consumableMoney: {runtimeConfig.gameSetup.consumableMoney}");
            Log.Msg($"spawnWithBikeOverride: {runtimeConfig.gameSetup.devSpawn}");

            var gameSetup = runtimeConfig.gameSetup;
            
            runtimeConfig.Seed = 7;
            gameSetup.police = false;
            gameSetup.startMoney = 0;
            gameSetup.respawnMoney = 0;
            gameSetup.consumableMoney = 0;
            gameSetup.gameMode = GameMode.RaceGameMode;
            gameSetup.scoreToWin = 99999;
            gameSetup.totalLives = 99999;

            // runtimeConfig.gameSetup = new RuntimeConfig.GameSetup() {
            //     police = false,
            //     spawnPickups = false,
            //     startMoney = 0,
            //     respawnMoney = 0,
            //     consumableMoney = 0,
            //     // devWeapon = "Bat",
            //     // devSecondary = "Bat",
            //     devSpawn = false,
            //     devBike = 4,
            //     spawnWithBikeOverride = -1,
            //     tutorialSpawn = false,
            //     gameMode = GameMode.RaceGameMode,
            //     spawnStragglerBoosts = true,
            //     stressTestMode = false,
            //     raceStart = true,
            //     scoreToWin = 10,
            //     totalLives = 77,
            // };

            runtimeConfig.Seed = 0;

            Log.Msg($"RuntimeConfig: {runtimeConfig.Dump()}");
            Log.Msg($"GameSetup: {runtimeConfig.gameSetup.ToString()}");
        }
    }

    internal static EntityRef? playerref = null;

    [HarmonyPatch(typeof(BikeRespawnSystem), "SpawnBike")]
    private class DisableAllRacingInhibitors
    {
        public static void Postfix(Frame f, EntityRef playerEntity, PlayerRef playerRef, Transform3D spawnTransform)
        {   
            Log.Msg($"{playerEntity.Index}, {playerEntity}");

            Il2CppSystem.Collections.Generic.List<EntityRef> refs = new();
            f.GetAllEntityRefs(refs);

            // playerref = playerEntity;

            // unsafe {
            //     Transform3D* t = f.GetPointer<Transform3D>(playerEntity);

            //     t->Position = new FPVector3();
            // }

            try
            {
                // foreach(var blocker in GameObject.FindObjectsOfType<QPrototypePathBlocker>())
                // {
                //     Log.Msg(blocker.name);

                //     foreach(var r in refs)
                //         if (r.Index == int.Parse(blocker.name.Split('.')[1]))
                //             f.Destroy(r);
                // }

                foreach (var entity in refs)
                {
                    if (f.Has<PathBlocker>(entity))
                        f.Destroy(entity);
                }
            }
            catch (System.Exception ex)
            {
                Log.Error(ex.ToString());
            }

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
                    if (enableTimer)
                    {
                        f.SystemDisable(sys);
                        Log.Msg("Disabled " + type.Name + " | ");
                    }
                    else
                    {
                        f.SystemEnable(type);
                        Log.Msg("Enabled " + type.Name + " | ");
                    }
                }
            }
        }
    }
}