using System.Collections.Generic;
using System.IO;
using Il2CppPhoton.Deterministic;

namespace TimerMod;

public abstract partial class ReadWrite
{
    internal static void CreateNewSeedFile(string QuickstopsFile)
        => File.WriteAllLines(QuickstopsFile, [
            "-",
            "-",
            "",
            "// First line is for the seed that determines all rng in a given race",
            "// Second line is what race segment to start on, may behave differetly on different maps",
        ]);


    internal static bool ReadSeed(out int seed)
    {
        var lines = File.ReadAllLines(Timer.SeedFile);
        return int.TryParse(lines[0], out seed);
    }

    internal static bool ReadArenaIndex(out int arenaIndex)
    {
        var lines = File.ReadAllLines(Timer.SeedFile);
        return int.TryParse(lines[1], out arenaIndex);
    }


    internal static bool VerifyStormDrainQuickstops(int seed)
    {
        RNGSession rng = new(seed);
        bool vF1 = true;
        bool vF2 = true;

        for (int f = 0; f < 33; f++)
        {
            FP value = rng.Next();

            if ((f == 30) && value > FP._0_50)
                vF1 = false;
            
            if ((f == 32) && value > FP._0_50)
                vF2 = false;
        }

        if (vF1 && vF2)
            return true;
        
        return false;
    }


    internal static List<(bool f1, bool f2, int seed)> ReadRNGFiles()
    {
        List<(bool f1, bool f2, int seed)> rngs = [];

        string[] files = Directory.GetFiles(Timer.RaceDataFolder);

        foreach (var file in files)
        {
            string[] lines = File.ReadAllLines(file);

            string s = file[(file.LastIndexOf('\\') + 1)..].Split('.')[0];
            Timer.Log.Msg(s);
            int seed = int.Parse(s);

            rngs.Add((lines[0][0] == 'Y', lines[0][2] == 'Y', seed));
        }

        return rngs;
    }
}