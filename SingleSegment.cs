using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppQuantum;

namespace TimerMod;

public class SingleSegment(Il2CppStructArray<Arena> arenas, int arenaIndex)
{
    public readonly Il2CppStructArray<Arena> Arenas = arenas;
    private readonly int index = arenaIndex;
    private int startArenaIndex;

    public int GetArenaIndex(int offset = 0)
    {
        int index = this.index + startArenaIndex + offset;

        while (index < 0)
            index += Arenas.Length;
        
        return index % Arenas.Length;
    }

    public Arena GetArena()
        => Arenas[this.GetArenaIndex()];
    
    public bool IsStartingLine()
        => GetArenaIndex() == startArenaIndex;


    public static SingleSegment Create(MapConfig mapConfig, int arenaIndex)
    {
        SingleSegment currArena = new(mapConfig.arenas, arenaIndex);

        for (int i = 0; i < mapConfig.arenas.Length; i++)
            if (mapConfig.arenas[i].arenaType == ArenaType.RaceStart)
            {
                currArena.startArenaIndex = i;
                break;
            }
        
        return currArena;
    }
}