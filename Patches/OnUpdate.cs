using HarmonyLib;
using Il2CppCustomUIRendering_Access;
using Il2CppQuantum;
using Il2CppQuantum.Core;
using Il2CppQuantum_Game;
using UnityEngine;
using Il2CppList = Il2CppSystem.Collections.Generic.List<Il2CppQuantum.EntityRef>;

namespace TimerMod;

public partial class Timer
{
    internal static ASCIILabel SprintText = null;
    internal static ASCIILabel SumText = null;
    internal static Color TextBaseColor => new(0.7f, 0.7f, 0.7f, 1.0f);
    internal static int TrySetupUI = 0;
}

[HarmonyPatch(typeof(FrameContext), nameof(FrameContext.OnFrameSimulationBegin))]
class FrameContext_OnFrameSimulationBegin_Patch
{
    public unsafe static void Postfix(FrameBase f)
    {
        if (!Timer.enabled || Timer.RetryInfo is not null) return;
        if (Timer.currentRace is not RaceInfo race) return;

        var gameState = f.GetOrAddSingletonPointer<RaceGameState>();
        
        if (Timer.segmentArena is null && gameState->mode == RaceGameStateMode.Arena)
        {
            gameState->mode = RaceGameStateMode.Countdown;
            gameState->countdownTimer = 12;
        }

        if (gameState->currArenaType == ArenaType.RaceStart)
        {
            if (Timer.segmentArena is SingleSegment arena)
            {
                GiveAllPlayersInifiniteScore((Frame)f);

                if (gameState->countdownTimer > 0 && gameState->countdownTimer < 45)
                {
                    gameState->currArenaIndex = arena.GetArenaIndex();
                    gameState->lastArenaIndex = arena.GetArenaIndex(-1);
                }
                else if (f is Frame frame)
                {
                    RaceGameModeSystem.SetStartLineGatesClosed(frame, arena.GetArenaIndex(), true);
                }
            }
        }
        else race.totalElapsedTime++;

        // Timer.Log.Msg($"{gameState->arenaStopsCounter}, ({gameState->lastArenaIndex}, {gameState->currArenaIndex}), {gameState->playersNotYetReachedArena}, {gameState->countdownTimer}");

        if (Timer.TrySetupUI > 0)
        {
            if (Timer.TrySetupUI == 1)
                CreateTimerLabels(out Timer.SprintText, out Timer.SumText);

            Timer.TrySetupUI--;
        }

        if (Timer.SprintText is null) return;

        if (!race.crossedFinishLine && gameState->mode == RaceGameStateMode.Race)
            Timer.SprintText.Text = $"{System.TimeSpan.FromSeconds((double)gameState->timeInCurrentMode / 45.0):mm\\:ss\\.ff}";

        Timer.SprintText.freeColor = Timer.TextColor(Timer.wasFastestSprint);

        UpdateLabel(Timer.SprintText);
        UpdateLabel(Timer.SumText, race.RaceSumTime(), Timer.TextColor(Timer.wasFastestRaceSum));
    }

    private unsafe static void GiveAllPlayersInifiniteScore(Frame f)
    {
        Il2CppList refs = new();
        f.GetAllEntityRefs(refs);

        foreach (var entity in refs)
            if (f.Has<ParticipatingPlayer>(entity))
            {
                var p = f.GetPointer<ParticipatingPlayer>(entity);
                p->points = 999999;
                p->kills = 999999;
            }
    }

    internal static void UpdateLabel(ASCIILabel label, long? time = null, Color? color = null)
    {
        if (label is null) return;

        if (time is long t) label.Text = $"{System.TimeSpan.FromSeconds((double)t / 45.0):mm\\:ss\\.ff}";
        if (color is Color c) label.freeColor = c;
        
        // Timer.SumText.Text = $"{System.TimeSpan.FromSeconds((double)label2Time / 45.0):mm\\:ss\\.ff}";
        // Timer.SumText.freeColor = Timer.TextColor(Timer.wasFastestRaceSum);

        label.Resized();
        // Timer.SumText.Resized();

        label.gameObject.SetActive(false);
        // Timer.SumText.gameObject.SetActive(false);
        label.gameObject.SetActive(true);
        // Timer.SumText.gameObject.SetActive(true);
    }

    private static void CreateTimerLabels(out ASCIILabel? label1, out ASCIILabel? label2)
    {
        label1 = null;
        label2 = null;

        try
        {
            Transform scoreboardTransform = GameObject.Find("Scoreboard_Scorebox(Clone)").transform;
            ASCIILabel txt = null;

            foreach (var child in scoreboardTransform)
                if (child.TryCast<Transform>() is Transform tra && tra.name == "NameLabel")
                {
                    if (tra.TryGetComponent<ASCIILabel>(out var agh))
                    {
                        txt = agh;
                        // Timer.Log.Msg("Found a match!");
                    }
                }

            if (txt is not null)
            {
                var obj = UnityEngine.Object.Instantiate(txt.transform.gameObject, scoreboardTransform);
                var obj2 = UnityEngine.Object.Instantiate(txt.transform.gameObject, scoreboardTransform);
                
                obj.transform.localPosition = new Vector3(170, -48, 0);
                obj2.transform.localPosition = new Vector3(170, -98, 0);

                obj.transform.localScale = new Vector3(2.4f, 1.97f, 1.0f);
                obj2.transform.localScale = new Vector3(2.35f, 1.93f, 1.0f);

                label1 = obj.GetComponent<ASCIILabel>();
                label2 = obj2.GetComponent<ASCIILabel>();

                label1.Text = "00:00.00";
                label2.Text = "00:00.00";

                label1.freeColorMode = true;
                label2.freeColorMode = true;

                label1.gameObject.SetActive(false);
                label2.gameObject.SetActive(false);
                label1.gameObject.SetActive(true);
                label2.gameObject.SetActive(true);

                label1.freeColor = Timer.TextBaseColor;
                label2.freeColor = Timer.TextBaseColor;
            }
            else
            {
                Timer.Log.Error("Could not match text object :(");
            }
        }
        catch (System.Exception ex)
        {
            Timer.Log.Error(ex);
        }
    }
}
