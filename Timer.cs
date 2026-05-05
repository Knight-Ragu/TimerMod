using System;
using System.Collections.Generic;
using HarmonyLib;
using Il2CppQuantum;
using MelonLoader;

[assembly: MelonInfo(typeof(TimerMod.Timer), "TimerMod", "0.0.1", "Knight-Ragu", null)]
[assembly: MelonGame("Videocult", "Airframe")]

namespace TimerMod;

public partial class Timer : MelonMod
{
    internal static MelonLogger.Instance Log => Melon<Timer>.Instance.LoggerInstance;

    const string folderName = "times";


    internal static bool enableTimer = true;

    internal static double Now = 0.0;
    internal static double? RaceStart = null;
    internal static double fastestRace = 0.0;
    internal static double fastestSum = 0.0;

    internal static List<(double start, double end)> RaceTimes = [];

    internal static double RaceSum()
    {
        double sum = 0.0;

        foreach (var (start, end) in RaceTimes)
            sum += end - start;

        return sum;
    }
}


// public class RemovePathBlockers : SystemMainThreadFilter<BikeRespawnSystem.Filter>
// {
//     public RemovePathBlockers() {}
//     public RemovePathBlockers(System.IntPtr pointer) {}


//     public override void Update(Frame f)
//     {
//         Il2CppSystem.Collections.Generic.List<EntityRef> refs = new();
//         f.GetAllEntityRefs(refs);

//         foreach(var r in refs)
//         {
//             TimerMod.Log.Msg($"ref: {r.Index}");
//         }

//         try
//         {
//             foreach (var blocker in TimerMod.pathBlockers)
//             {
//                 foreach(var r in refs)
//                     if (r.Index == blocker)
//                         f.Destroy(r);
//             }
//         }
//         catch (System.Exception ex)
//         {
//             TimerMod.Log.Error(ex.ToString());
//         }
//     }
// }
