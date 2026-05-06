using HarmonyLib;
using Il2Cpp;
using Il2CppQuantum;

namespace TimerMod;

[HarmonyPatch(typeof(PhotonController), "ChangeSceneAndStartQuantumGameCoroutine")]
class PhotonController_ChangeSceneAndStartQuantumGameCoroutine_Patch
{
    public static void Prefix(ref RuntimeConfig runtimeConfig)
    {
        if (!Timer.enabled) return;

        var gameSetup = runtimeConfig.gameSetup;
        
        if (Timer.ReadSeedFile() is int seed)
            runtimeConfig.Seed = seed;

        gameSetup.police = false;
        gameSetup.startMoney = 9999999;
        gameSetup.respawnMoney = 9999999;
        gameSetup.consumableMoney = 9999999;
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

        Timer.Log.Msg($"Race Seed: {runtimeConfig.Seed}");
    }
}