using System.IO;
using HarmonyLib;
using Il2Cpp;
using Il2CppQuantum;
using Il2CppQuantum_Game;

namespace TimerMod;

[HarmonyPatch(typeof(RaceGameModeSystem), nameof(RaceGameModeSystem.ChangeMode))]
class RaceGameStateExtensions_RaceGameStateExtensions_Patch
{
    public unsafe static void Postfix(MapConfig mapConfig, RaceGameState* gameState)
    {
        if (Timer.RetryInfo is not Retry retry) return;

        if (
            retry.Type != RetryMethod.InfiniteRandomSeed
         && retry.Type != RetryMethod.RandomQuickstopSeed
        ) return;

        // Check to see if quickstops are going to happen in the current map

        bool quickstopsCorrect = true;

        for (int i = 0; i < gameState->quickStopArenaToggles.Length; i++)
        {
            Timer.Log.Msg($"{gameState->quickStopArenaToggles.GetPointer(i)->Value}: {retry.QuickstopToggles[i]}");

            if (retry.QuickstopToggles[i] == Quickstop.Ignore) continue;

            if (gameState->quickStopArenaToggles.GetPointer(i)->Value != (int)retry.QuickstopToggles[i])
            {
                quickstopsCorrect = false;
                break;
            }
        }

        if (quickstopsCorrect)
        {
            if (retry.Type != RetryMethod.InfiniteRandomSeed)
                File.WriteAllLines($"{Timer.SeedsFolder}\\{retry.Seed}", []);

            Timer.RetryInfo = null;

            Timer.Log.Msg($"{retry.Seed} - Valid!");
        }
        else
        {
            // Go to Main Menu and Permeate the cycle
            PhotonController.instance.LeaveRoom();

            Timer.Log.Msg($"{retry.Seed} - Invalid,");
        }

        // Convert.ToString(256, 2)
    } 
}