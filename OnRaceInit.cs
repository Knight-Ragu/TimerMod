using HarmonyLib;
using Il2CppQuantum_Game;

namespace TimerMod;

public partial class Timer
{
    // [HarmonyPatch(typeof(UnityEngine.Debug), "Log", [typeof(Il2CppSystem.Object)])]
    // [HarmonyPatch(typeof(UnityEngine.Debug), "Log", [typeof(Il2CppSystem.Object), typeof(UnityEngine.Object)])]
    // [HarmonyPatch(typeof(UnityEngine.Debug), "LogWarning", [typeof(Il2CppSystem.Object)])]
    // [HarmonyPatch(typeof(UnityEngine.Debug), "LogWarning", [typeof(Il2CppSystem.Object), typeof(UnityEngine.Object)])]
    // [HarmonyPatch(typeof(UnityEngine.Debug), "LogError", [typeof(Il2CppSystem.Object)])]
    // [HarmonyPatch(typeof(UnityEngine.Debug), "LogError", [typeof(Il2CppSystem.Object), typeof(UnityEngine.Object)])]

    internal static int TrySetupUI = 0;

    [HarmonyPatch(typeof(StragglerBoostSystem), "OnInit")]
    private class InitUI
    {
        public static void Postfix() // Straggler boost positions:
        {
            TrySetupUI = 270;
            RaceTimes.Clear();

            fastestRace = 0.0;
            fastestSum = 0.0;
        }
    }
}