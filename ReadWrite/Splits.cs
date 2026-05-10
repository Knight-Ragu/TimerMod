using System.IO;
using Il2CppQuantum;
using UnityEngine.SceneManagement;

namespace TimerMod;

public partial class ReadWrite
{
    internal static void CreateNewSplitsFile(string splitsFile)
        => File.WriteAllLines(splitsFile, ["-|-", "-|-", "-|-", "-|-", "-|-", "-|-", "-|-", "-|-"]);

    internal static (string directory, string file) GetSplitsFile(Scene map, HoverbikeModel? bikeModel)
    {
        string model;

        if (bikeModel is not null)
            model = System.Enum.GetName(Timer.bikeModel.Value);
        else
            model = "Nobike";

        string splitsFolderName = $"\\{map.name}";
        string splitsFileName = $"\\{model}";

        return (Timer.TimesFolder + splitsFolderName, Timer.TimesFolder + splitsFolderName + splitsFileName);
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
                    wroteToSprint = savedSprint > sprintTime;
                    sprintTime = System.Math.Min(sprintTime, savedSprint);
                }

                if (double.TryParse(pair[1], out var savedRace))
                {
                    wroteToSum = savedRace > raceTime;
                    raceTime = System.Math.Min(raceTime, savedRace);
                }
            }
        }

        times[index] = $"{sprintTime}|{raceTime}";

        File.WriteAllLines(splitsFile, times);
        
        return (wroteToSprint, wroteToSum);
    }
} 