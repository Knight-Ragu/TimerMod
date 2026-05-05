using HarmonyLib;
using Il2CppQuantum_Game;

namespace TimerMod;

public partial class Timer
{
    [HarmonyPatch(typeof(RaceGameModeSystem), "EnterRaceMode")]
    private class EnterRaceMode
    {
        public static void Postfix() // RACE START
        {
            RaceStart = Now;
            Log.Msg("RaceStart set to Now!");
        }
    }
}