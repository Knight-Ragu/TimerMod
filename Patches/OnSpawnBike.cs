using HarmonyLib;
using Il2CppQuantum;
using Il2CppQuantum_Game;

namespace TimerMod;

public partial class Timer
{
    internal static EntityRef? playerref = null;

    [HarmonyPatch(typeof(BikeRespawnSystem), nameof(BikeRespawnSystem.SpawnBike))]
    private class BikeRespawnSystem_SpawnBike_Patch
    {
        public static void Postfix(Frame f, EntityRef playerEntity, PlayerRef playerRef, Transform3D spawnTransform)
        {   
            foreach(var sys in f.SystemsAll)
            {
                var type = sys.GetIl2CppType();

                if (
                    type.Name == "PoliceCoordinatorSystem"
                    || type.Name == "PoliceHelicopterSystem"
                    || type.Name == "PoliceGunmanSystem"

                    || type.Name == "PickupSpawnSystem"
                    || type.Name == "SpecialPickupSpawnSystem"
                ) {
                    if (enabled)
                    {
                        f.SystemDisable(sys);
                        // Log.Msg("Disabled " + type.Name + " | ");
                    }
                    else
                    {
                        f.SystemEnable(type);
                        // Log.Msg("Enabled " + type.Name + " | ");
                    }
                }
            }

            if (!Timer.enabled) return;

            Il2CppSystem.Collections.Generic.List<EntityRef> refs = new();
            f.GetAllEntityRefs(refs);

            foreach (var entity in refs)
            {
                if (f.Has<PathBlocker>(entity))
                    f.Destroy(entity);
            }
        }
    }
}