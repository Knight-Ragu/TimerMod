using HarmonyLib;
using Il2CppQuantum;
using Il2CppView_Entities;

namespace TimerMod;

public partial class Timer
{
    internal static HoverbikeModel? bikeModel = null;
}

[HarmonyPatch(typeof(EngineSounds), nameof(EngineSounds.Instantiate))]
class EngineSounds_Instantiate_Patch
{
    public static void Postfix(HoverbikeModel model)
    {
        Timer.bikeModel = model;
        // Timer.Log.Msg($"Bike Model: {model}");
    }
}