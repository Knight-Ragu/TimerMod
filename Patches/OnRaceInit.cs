using HarmonyLib;
using Il2CppQuantum_Game;

namespace TimerMod;

[HarmonyPatch(typeof(StragglerBoostSystem), nameof(StragglerBoostSystem.OnInit))]
class StragglerBoostSystem_OnInit_Patch
{
    public static void Postfix() // Straggler boost positions:
    {
        if (!Timer.enabled) return;

        Timer.TrySetupUI = 270;
        Timer.SprintTimes.Clear();

        Timer.wasFastestSprint = false;
        Timer.wasFastestRaceSum = false;
    }
}