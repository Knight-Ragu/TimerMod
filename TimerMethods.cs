using System.IO;
using Il2CppQuantum;
using UnityEngine.SceneManagement;

namespace TimerMod;

public partial class Timer
{
    internal static void Reset()
    {
        Timer.RaceText = null;
        Timer.SumText = null;

        Timer.RaceStart = null;
        Timer.bikeModel = null;
        Timer.SprintTimes.Clear();

        Timer.wasFastestSprint = false;
        Timer.wasFastestRaceSum = false;
        Timer.Now = 0.0;
    }


    internal static void CreateNewSplitsFile(string splitsFile)
        => File.WriteAllLines(splitsFile, ["-|-", "-|-", "-|-", "-|-", "-|-", "-|-", "-|-", "-|-"]);

    internal static string GetSplitsFile(Scene map, HoverbikeModel? bikeModel)
    {
        string model;

        if (bikeModel is not null)
            model = System.Enum.GetName(Timer.bikeModel.Value);
        else
            model = "Nobike";

        string splitsFileName = $"\\{SceneManager.GetActiveScene().name} - {model}.txt";
        string splitsFile = Timer.DataFolder + splitsFileName;

        return splitsFile;
    }

    internal static (bool wroteToSprint, bool wroteToSum) SaveSplitTime(string splitsFile, int index, double sprintTime, double raceTime)
    {
        var times = File.ReadAllLines(splitsFile);

        { // Resize Array
            if (index >= times.Length) System.Array.Resize(ref times, index + 1);

            for (int f = 0; f < times.Length; f++)
                if (times[f] is null) times[f] = "";
        }

        bool wroteToSprint = true;
        bool wroteToSum = true;

        { // Decide what to write
            var pair = times[index].Split("|");

            if (pair.Length == 2)
            {
                if (double.TryParse(pair[0], out var savedSprint))
                {
                    wroteToSprint = savedSprint > raceTime;
                    sprintTime = System.Math.Min(sprintTime, savedSprint);
                }

                if (double.TryParse(pair[1], out var savedRace))
                {
                    wroteToSum = savedRace > sprintTime;
                    raceTime = System.Math.Min(raceTime, savedRace);
                }
            }
        }

        var line = $"{sprintTime}|{raceTime}";
        times[index] = line;

        File.WriteAllLines(splitsFile, times);
        
        return (wroteToSprint, wroteToSum);
    }


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

        double totalTime = Timer.Now;

        { // Decide what to write
            if (double.TryParse(times[i].Split(' ')[0], out var num))
                totalTime = System.TimeSpan.FromHours(num).TotalSeconds + Timer.Now;
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