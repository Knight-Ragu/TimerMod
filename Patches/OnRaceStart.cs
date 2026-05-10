using HarmonyLib;
using Il2CppQuantum_Game;

namespace TimerMod;

public partial class Timer
{
    [HarmonyPatch(typeof(RaceGameModeSystem), nameof(RaceGameModeSystem.EnterRaceMode))]
    private class RaceGameModeSystem_EnterRaceMode_Patch
    {
        public static void Postfix() // RACE START
        {
            SprintStart = Now;
            Log.Msg("RaceStart set to Now!");
        }
    }
}