using System.Reflection;
using System.IO;
using HarmonyLib;
using Il2CppQuantum;

namespace TimerMod;

public partial class Timer
{
    [HarmonyPatch(typeof(SessionRunner), "Shutdown")]
    private class Shutdown
    {
        public static void Postfix() // Shutting down runner
        {
            if (Now == 0.0) return;

            Log.Msg($"Game time: {System.TimeSpan.FromSeconds(Now):mm\\:ss\\.ff}");
            
            string path = Assembly.GetExecutingAssembly().Location;
            path = path[..(path.LastIndexOf('\\') + 1)];
            path += folderName;

            string playtimesFile = path + $"\\Playtimes.txt";

            Log.Msg(playtimesFile);

            try
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                if (!File.Exists(playtimesFile))
                    File.WriteAllLines(playtimesFile, NewPlaytimesFile());

                File.WriteAllLines(playtimesFile, AppendPlaytime(playtimesFile, bikeModel));
            }
            catch (System.Exception ex)
            {
                Log.Error(ex);            
            }

            RaceText = null;
            SumText = null;

            RaceStart = null;
            bikeModel = null;
            RaceTimes.Clear();

            fastestRace = 0.0;
            fastestSum = 0.0;
            Now = 0.0;
        }
    }

    public static string[] NewPlaytimesFile()
        => ["0.0", "0.0", "0.0", "0.0", "0.0", " ", "0.0"];

    public static string[] AppendPlaytime(string playtimesFile, HoverbikeModel? bM)
    {
        var times = File.ReadAllLines(playtimesFile);

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

        double totalTime = Now;

        { // Decide what to write
            if (double.TryParse(times[i].Split(' ')[0], out var num))
                totalTime = System.TimeSpan.FromHours(num).TotalSeconds + Now;
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

        return times;
    }
}