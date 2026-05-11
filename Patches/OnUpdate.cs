using HarmonyLib;
using Il2CppCustomUIRendering_Access;
using Il2CppPhoton.Deterministic;
using Il2CppQuantum;
using Il2CppQuantum.Core;
using UnityEngine;

namespace TimerMod;

public partial class Timer
{
    internal static ASCIILabel SprintText = null;
    internal static ASCIILabel SumText = null;
    internal static Color TextBaseColor => new(0.7f, 0.7f, 0.7f, 1.0f);
    internal static int TrySetupUI = 0;
}

[HarmonyPatch(typeof(DeterministicSession), nameof(DeterministicSession.UpdateSimulationInner))]
class DeterministicSession_UpdateSimulationInner_Patch
{
    public static void Postfix(DeterministicSession __instance)
    {
        if (!Timer.enabled || Timer.RetryInfo is not null) return;

        if (Timer.TrySetupUI > 0)
        {
            if (Timer.TrySetupUI == 1)
            {
                Timer.Log.Msg($"DeltaTime: {__instance.DeltaTime}");

                try 
                {
                    var hud = GameObject.Find("Scoreboard_Scorebox(Clone)").transform;
                    ASCIILabel txt = null;

                    foreach (var child in hud)
                    {
                        if (child.TryCast<Transform>() is Transform tra)
                        {
                            try {
                                if (tra.name == "NameLabel" && tra.TryGetComponent<ASCIILabel>(out var agh))
                                {
                                    txt = agh;
                                    Timer.Log.Msg("Found a match!");
                                }
                            } 
                            catch (System.Exception ex)
                            {
                                Timer.Log.Error(ex);
                            }
                        }
                        else
                        {
                            Timer.Log.Error("Object not transform");
                        }
                    }

                    if (txt is not null)
                    {
                        var obj = UnityEngine.Object.Instantiate(txt.transform.gameObject, hud);
                        var obj2 = UnityEngine.Object.Instantiate(txt.transform.gameObject, hud);
                        
                        obj.transform.localPosition = new Vector3(170, -48, 0);
                        obj2.transform.localPosition = new Vector3(170, -98, 0);

                        obj.transform.localScale = new Vector3(2.4f, 1.97f, 1.0f);
                        obj2.transform.localScale = new Vector3(2.35f, 1.93f, 1.0f);

                        Timer.SprintText = obj.GetComponent<ASCIILabel>();
                        Timer.SumText = obj2.GetComponent<ASCIILabel>();

                        Timer.SprintText.Text = "00:00.00";
                        Timer.SumText.Text = "00:00.00";

                        Timer.SprintText.freeColorMode = true;
                        Timer.SumText.freeColorMode = true;

                        Timer.SprintText.gameObject.SetActive(false);
                        Timer.SumText.gameObject.SetActive(false);
                        Timer.SprintText.gameObject.SetActive(true);
                        Timer.SumText.gameObject.SetActive(true);

                        Timer.SprintText.freeColor = Timer.TextBaseColor;
                        Timer.SumText.freeColor = Timer.TextBaseColor;
                    }
                    else
                    {
                        Timer.Log.Msg("Could not match text object :(");
                    }
                }
                catch (System.Exception ex)
                {
                    Timer.Log.Error(ex);
                }
            }

            Timer.TrySetupUI--;
        }


        if (Timer.SprintText is null) return;

        Timer.SumText.Text = $"{System.TimeSpan.FromSeconds(Timer.RaceSumSeconds()):mm\\:ss\\.ff}";
        Timer.SumText.freeColor = Timer.TextColor(Timer.wasFastestRaceSum);

        Timer.SprintText.Text = $"{System.TimeSpan.FromSeconds(Timer.SprintSeconds):mm\\:ss\\.ff}";
        Timer.SprintText.freeColor = Timer.TextColor(Timer.wasFastestSprint);

        Timer.SprintText.Resized();
        Timer.SumText.Resized();

        Timer.SprintText.gameObject.SetActive(false);
        Timer.SumText.gameObject.SetActive(false);
        Timer.SprintText.gameObject.SetActive(true);
        Timer.SumText.gameObject.SetActive(true);
    }
}

// [HarmonyPatch(typeof(FrameContext), nameof(FrameContext.OnFrameSimulationBegin))]
class FrameContext_OnFrameSimulationBegin_Patch
{
    public unsafe static void Postfix(FrameBase f)
    {
        if (!Timer.enabled) return;

        Timer.totalTime++;

        var gameState = f.GetOrAddSingletonPointer<RaceGameState>();

        if (gameState->mode == RaceGameStateMode.Race && !Timer.crossedFinishLine)
            Timer.sprintTime = gameState->timeInCurrentMode;
        
        gameState->currArenaType = ArenaType.RaceStart;
        
        // if (gameState->mode == RaceGameStateMode.Arena)
        // {
        //     gameState->mode = RaceGameStateMode.Countdown;
        //     gameState->countdownTimer = 7;
        // }

        if (gameState->currArenaType != ArenaType.RaceStart)
        {
            // Timer.Log.Msg($"countdownTimer: {gameState->countdownTimer}");
        }

        // Timer.Log.Msg($"{gameState->arenaStopsCounter}, ({gameState->lastArenaIndex}, {gameState->currArenaIndex}), {gameState->playersNotYetReachedArena}, {gameState->countdownTimer}");
    }
}