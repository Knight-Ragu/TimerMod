using HarmonyLib;
using Il2CppQuantum;
using Il2CppView_Entities;

namespace TimerMod;

public partial class Timer
{
    internal static HoverbikeModel? bikeModel = null;
    
    [HarmonyPatch(typeof(EngineSounds), "Instantiate")]
    private class GetBike
    {
        public static void Postfix(HoverbikeModel model)
        {
            bikeModel = model;
            Log.Msg($"Bike Model: {model}");
        }
    }
}