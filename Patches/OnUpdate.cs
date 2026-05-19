using HarmonyLib;
using Il2CppQuantum;
using Il2CppQuantum.Core;
using Il2CppQuantum_Game;
using Il2CppList = Il2CppSystem.Collections.Generic.List<Il2CppQuantum.EntityRef>;

namespace TimerMod;

[HarmonyPatch(typeof(FrameContext), nameof(FrameContext.OnFrameSimulationBegin))]
class FrameContext_OnFrameSimulationBegin_Patch
{
    public unsafe static void Postfix(FrameBase f)
    {
        if (!Timer.enabled || Timer.RetryInfo is not null) return;
        if (Timer.CurrentRace is not RaceInfo race) return;

        race.totalElapsedTime++;

        var gameState = f.GetOrAddSingletonPointer<RaceGameState>();
        
        if (Timer.Segment is null && gameState->mode == RaceGameStateMode.Arena)
        {
            gameState->mode = RaceGameStateMode.Countdown;
            gameState->countdownTimer = 12;
        }

        if (gameState->currArenaType == ArenaType.RaceStart)
        {
            if (Timer.Segment is SingleSegment arena && f.TryCast<Frame>() is Frame frame)
            {
                GiveAllPlayersInifiniteScore(frame);

                if (gameState->countdownTimer > 0 && gameState->countdownTimer < 45)
                {
                    gameState->currArenaIndex = arena.ArenaIndex();
                    gameState->lastArenaIndex = arena.ArenaIndex(-1);
                }
                else RaceGameModeSystem.SetStartLineGatesClosed(frame, arena.ArenaIndex(), true);
            }
        }

        if (
            !race.crossedFinishLine
            && gameState->mode == RaceGameStateMode.Race
            && Timer.LabelManager.TryGetLabel(0, out var l0)
        )
            l0.PrimaryText = gameState->timeInCurrentMode;

        if (Timer.LabelManager.TryGetLabel(1, out var l1))
            l1.PrimaryText = race.RaceSumTime();
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
}
