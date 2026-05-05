using System.IO;
using System.Reflection;
using HarmonyLib;
using Il2CppQuantum_Game;
using UnityEngine.SceneManagement;

namespace TimerMod;

public partial class Timer
{
    [HarmonyPatch(typeof(RaceGameModeSystem), "CrossedFinishLine")]
    private class CrossedFinishLine
    {
        public static void Postfix() // crossed the finish line
        {
            if (RaceStart is double t)
            {
                RaceTimes.Add((t, Now));

                Log.Msg($"Race time: {System.TimeSpan.FromSeconds(Now - t):mm\\:ss\\.ff}, Total game time: {System.TimeSpan.FromSeconds(Now):mm\\:ss\\.ff}");
            }
            else
            {
                Log.Msg("RaceStart is null..?");
            }

            var (start, end) = RaceTimes[^1];

            // Z:\home\knightragu\Documents\Airframe Ultra Playtest 3\Airframe Ultra Playtest\BepInEx\plugins\AddAirToFrame.dll
            string path = Assembly.GetExecutingAssembly().Location;
            path = path.Remove(path.LastIndexOf('\\') + 1);
            path += folderName;

            string model;

            if (bikeModel is not null)
                model = System.Enum.GetName(bikeModel.Value);
            else
                model = "Nobike";

            string mapFile = path + $"\\{SceneManager.GetActiveScene().name} - {model}.txt";

            Log.Msg(mapFile);

        TryAgain:

            if (Directory.Exists(path))
            {
                double raceTime = end - start;
                double sumTime = RaceSum();

                fastestRace = double.MaxValue;
                fastestSum = double.MaxValue;

                if (File.Exists(mapFile))
                {
                    var times = File.ReadAllLines(mapFile);

                    int i = RaceTimes.Count - 1;

                    { // Resize Array
                        if (i >= times.Length) System.Array.Resize(ref times, i + 1);

                        for (int f = 0; f < times.Length; f++)
                            if (times[f] is null) times[f] = "";
                    }

                    { // Decide what to write
                        var pair = times[i].Split("|");

                        if (pair.Length == 2)
                        {
                            if (double.TryParse(pair[0], out var num))
                            {
                                raceTime = System.Math.Min(raceTime, num);
                                fastestRace = num;
                            }
                            

                            if (double.TryParse(pair[1], out var num2))
                            {
                                sumTime = System.Math.Min(sumTime, num2);
                                fastestSum = num2;
                            }
                        }
                    }

                    times[i] = $"{raceTime}|{sumTime}";

                    Log.Msg($"{raceTime}|{sumTime}");

                    File.WriteAllLines(mapFile, times);
                }
                else
                {
                    File.WriteAllLines(mapFile, [$"{raceTime}|{sumTime}"]);
                }
            }
            else
            {
                Directory.CreateDirectory(path);

                goto TryAgain;
            }

            RaceStart = null;
        }
    }
}