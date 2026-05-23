using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppQuantum;

namespace TimerMod;

public class SingleSegment(Il2CppStructArray<Arena> arenas, int arenaIndex)
{
    public readonly Il2CppStructArray<Arena> Arenas = arenas;
    public int StartingLine;
    private readonly int index = arenaIndex;

    public int ArenaIndex(int offset = 0)
    {
        int index = this.index + StartingLine + offset;

        while (index < 0)
            index += Arenas.Length;
        
        return index % Arenas.Length;
    }

    public Arena Arena(int offset = 0)
        => Arenas[this.ArenaIndex(offset)];
    
    public bool IsStartingLine()
        => ArenaIndex() == StartingLine;


    public static SingleSegment Create(MapConfig mapConfig, int arenaIndex)
    {
        SingleSegment currArena = new(mapConfig.arenas, arenaIndex);

        for (int i = 0; i < mapConfig.arenas.Length; i++)
            if (mapConfig.arenas[i].arenaType == ArenaType.RaceStart)
            {
                currArena.StartingLine = i;
                break;
            }
        
        return currArena;
    }
}