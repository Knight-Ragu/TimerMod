using System.IO;
using Il2CppQuantum;

namespace TimerMod;

public partial class ReadWrite
{
    internal static void CreateNewPlaytimesFile(string filePath)
        => File.WriteAllLines(filePath, ["0.0", "0.0", "0.0", "0.0", "0.0", " ", "0.0"]);

    internal static string SavePlaytime(string playtimeFile, HoverbikeModel? bM)
    {
        var times = File.ReadAllLines(playtimeFile);

        int i;
        string model;

        if (bM is not null)
        {
            i = (int)bM;
            model = System.Enum.GetName(bM.Value);
        }
        else
        {
            i = 4;
            model = "Nobike";
        }

        double totalTime = Timer.TotalSeconds;

        { // Decide what to write
            if (double.TryParse(times[i].Split(' ')[0], out var num))
                totalTime += System.TimeSpan.FromHours(num).TotalSeconds;
        }
        

        times[i] = $"{System.TimeSpan.FromSeconds(totalTime).TotalHours} // {model}";

        double sum = 0.0;

        for (int f = 0; f < 4; f++)
        {
            if (double.TryParse(times[f].Split(' ')[0], out var num))
            {
                sum += System.TimeSpan.FromHours(num).TotalSeconds;
            }
        }

        times[6] = $"{System.TimeSpan.FromSeconds(sum).TotalHours} // Total Playtime";

        File.WriteAllLines(playtimeFile, times);

        return times[i];
    }
}