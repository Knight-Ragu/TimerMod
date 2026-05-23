using HarmonyLib;
using Il2Cpp;
using Il2CppQuantum;

namespace TimerMod;

[HarmonyPatch(typeof(PhotonController), nameof(PhotonController.ChangeSceneAndStartQuantumGameCoroutine))]
class PhotonController_ChangeSceneAndStartQuantumGameCoroutine_Patch
{
    public static void Prefix(ref RuntimeConfig runtimeConfig)
    {
        Timer.Reset();

        int seed = runtimeConfig.Seed;

        if (Timer.RetryInfo is Retry retry)
        {
            if (retry.Type == RetryMethod.SameSeed) seed = Timer.LastConfig.Seed;

            runtimeConfig.Map = Timer.LastConfig.Map;
            runtimeConfig.SimulationConfig = Timer.LastConfig.SimulationConfig;
            runtimeConfig.SystemsConfig = Timer.LastConfig.SystemsConfig;
            runtimeConfig.config = Timer.LastConfig.config;
            runtimeConfig.gameSetup = Timer.LastConfig.gameSetup;

            Timer.LastConfig = null;

            Timer.Log.Msg($"Retry: {retry.Type}");

            if (
                retry.Type != RetryMethod.InfiniteRandomSeed
             && retry.Type != RetryMethod.RandomSetQuickstopsSeed
            ) {
                Timer.RetryInfo = null;
            } else { 
                retry.Seed = seed;
            }
        } 
        else if (ReadWrite.ReadSeed(out int seedFile))
            seed = seedFile;
        

        var gameSetup = runtimeConfig.gameSetup;

        if (ReadWrite.ReadArenaIndex(out _))
            gameSetup.scoreToWin = 1;
        
        gameSetup.police = false;
        gameSetup.startMoney = 999999;
        gameSetup.respawnMoney = 999999;
        gameSetup.consumableMoney = 999999;
        gameSetup.totalLives = 999999;

        runtimeConfig.Seed = seed;
        Timer.LastConfig = runtimeConfig;

        Timer.Log.Msg($"Race Seed: {runtimeConfig.Seed}");

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
    }
}