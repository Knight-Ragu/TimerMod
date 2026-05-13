using HarmonyLib;
using Il2CppPhoton.Deterministic;
using Il2CppQuantum;
using Il2CppQuantum_Game;

namespace TimerMod;

[HarmonyPatch(typeof(BikeRespawnSystem), nameof(BikeRespawnSystem.SpawnBike))]
class BikeRespawnSystem_SpawnBike_Patch
{
    public static void Prefix(ref Transform3D spawnTransform)
    {   
        if (Timer.SpawnPosition.Position != FPVector3.Zero)
        {
            spawnTransform = Timer.SpawnPosition;
            Timer.SpawnPosition.Position = FPVector3.Zero;
        }
    }
}