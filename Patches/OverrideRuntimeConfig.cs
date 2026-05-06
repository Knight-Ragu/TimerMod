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

        Timer.Log.Msg($"RuntimeConfig Seed: {runtimeConfig.Seed}");

        var gameSetup = runtimeConfig.gameSetup;
        
        // runtimeConfig.Seed = -2070715567;

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

        Timer.Log.Msg($"RuntimeConfig: {runtimeConfig.Dump()}");
    }
}