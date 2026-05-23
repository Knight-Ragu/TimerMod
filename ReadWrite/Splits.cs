using System;
using System.IO;
using Il2CppQuantum;

namespace TimerMod;

public partial class ReadWrite
{
    internal static string BikeModelName(HoverbikeModel? bikeModel)
    {
        string model;

        if (bikeModel is not null)
            model = System.Enum.GetName(bikeModel.Value);
        else
            model = "Nobike";
        
        return model;
    }

    internal static void CreateNewSplitsFile(string splitsFile, string mapName, HoverbikeModel? bikeModel)
        => File.WriteAllLines(splitsFile, [$"// {BikeModelName(bikeModel)} - {mapName}"]);

    internal static string GetSingleSegmentFilePath(string mapName, HoverbikeModel? bikeModel)
    {
        string splitsFolderName = $"\\{mapName}";
        string splitsFileName = $"\\{BikeModelName(bikeModel)}";

        string splitsFilePath = Timer.TimesFolder + splitsFolderName + splitsFileName;

        if (!Directory.Exists(Timer.TimesFolder + splitsFolderName))
                Directory.CreateDirectory(Timer.TimesFolder + splitsFolderName);

        if (!File.Exists(splitsFilePath))
            ReadWrite.CreateNewSplitsFile(splitsFilePath, mapName, bikeModel);

        return splitsFilePath;
    }

    internal static unsafe bool SaveSingleSegmentTimeIfFaster(string splitsFile, SingleSegment segment, RaceGameState* gameState)
    {
        string segmentID = $"{gameState->lastArenaIndex}-{gameState->currArenaIndex}";
        Timer .Log.Msg($"segmentID: {segmentID}"); 

        string[] times = File.ReadAllLines(splitsFile);

        int lineToEdit = 64;
        bool wroteToFile = true;

        for (int i = 0; i < times.Length; i++)
        {
            var split = times[i].Split('|');
            if (split[0] == segmentID)
            {
                lineToEdit = i;
                break;
            }
        }

        if (lineToEdit == 64) // Grow Array
        { 
            lineToEdit = times.Length;
            
            Array.Resize(ref times, times.Length + 1);
            times[lineToEdit] = "";
        }


        long sprintTime = gameState->timeInCurrentMode;

        var line = times[lineToEdit].Split('|');
        if (line.Length >= 2 && long.TryParse(line[1], out var savedSprintTime))
        {
            sprintTime = Math.Min(sprintTime, savedSprintTime);
            if (sprintTime >= savedSprintTime) wroteToFile = false;
        }
        
        times[lineToEdit] = $"{segmentID}|{sprintTime}";
        File.WriteAllLines(splitsFile, times);
        
        return wroteToFile;
    }

    internal static unsafe (bool wroteToSprint, bool wroteToSum) SaveRaceTimeIfFaster(string splitsFile, RaceGameState* gameState, RaceInfo currentRace)
    {
        int index = gameState->lastArenaIndex;
        long sprintTime = gameState->timeInCurrentMode;
        long raceTime = currentRace.RaceSumTime();

        var times = File.ReadAllLines(splitsFile);

        { // Resize Array
            if (index >= times.Length) System.Array.Resize(ref times, index + 1);

            for (int f = 0; f < times.Length; f++)
                if (times[f] is null) times[f] = "-|-";
        }

        bool wroteToSprint = true;
        bool wroteToSum = true;

        { // Decide what to write
            var pair = times[index].Split("|");

            if (pair.Length == 2)
            {
                if (long.TryParse(pair[0], out var savedSprint))
                {
                    wroteToSprint = savedSprint > sprintTime;
                    sprintTime = System.Math.Min(sprintTime, savedSprint);
                }

                if (long.TryParse(pair[1], out var savedRace))
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