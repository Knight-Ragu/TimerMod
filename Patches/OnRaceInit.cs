using HarmonyLib;
using Il2CppQuantum_Game;

namespace TimerMod;

[HarmonyPatch(typeof(RaceGameModeSystem), nameof(RaceGameModeSystem.OnInit))]
class StragglerBoostSystem_OnInit_Patch
{
    public static void Postfix() // Straggler boost positions:
    {
        if (!Timer.enabled) return;

        Timer.TrySetupUI = 270;
        Timer.Reset();
    }
}