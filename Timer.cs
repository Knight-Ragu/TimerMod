using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine.SceneManagement;
using MelonLoader;
using Il2CppQuantum;
using Il2CppPhoton.Deterministic;
using Il2CppMenus;
using Il2CppTMPro;
using Il2CppView_Entities;
using Il2CppQuantum_Game;
using Il2CppCustomUIRendering_Access;
using Il2CppView_Environment;
using System;

[assembly: MelonInfo(typeof(TimerMod.Timer), "TimerMod", "0.0.1", "Knight-Ragu", null)]
[assembly: MelonGame("Videocult", "Airframe")]

namespace TimerMod;

public class Timer : MelonMod
{
    private MelonPreferences_Category MultiplayerConfigOptions;
    private MelonPreferences_Entry<bool> RemoveEstateDoor;

    public override void OnInitializeMelon()
    {
        MultiplayerConfigOptions = MelonPreferences.CreateCategory("MultiplayerConfigOptions");
        RemoveEstateDoor = MultiplayerConfigOptions.CreateEntry<bool>("RemoveEstateDoor", true);
    }
    
    const string guid = "knightragu.timermod";

    const string folderName = "times";

    internal static MelonLogger.Instance Log => Melon<Timer>.Instance.LoggerInstance;

    internal static bool enableTimer = true;

    internal static double? RaceStart = null;
    internal static List<(double start, double end)> RaceTimes = [];

    internal static double RaceSum()
    {
        double sum = 0.0;

        foreach (var (start, end) in RaceTimes)
            sum += end - start;

        return sum;
    }

    internal static Toggle? toggle;

    [HarmonyPatch(typeof(AirframeMainMenu), "Update")]
    private static class MainMenuUpdate
    {
        public static void Postfix()
        { 
            var lookFor = "SettingsScrollview";

            if (toggle == null)
            {
                if (GameObject.Find(lookFor) is GameObject gameObj)
                {
                    var option = gameObj.transform.GetChild(0).GetChild(0).GetChild(3).gameObject;
                    var timerOption = GameObject.Instantiate(option);

                    timerOption.transform.SetParent(option.transform.parent, false);
                    timerOption.transform.localScale = Vector3.one;

                    if (timerOption.transform.GetChild(0)?.GetComponent<TextMeshProUGUI>() is TextMeshProUGUI text)
                        text.text = "Timer";

                    if (timerOption.GetComponentInChildren<Toggle>() is Toggle tog)
                        toggle = tog;
                }
            } else
            {
                enableTimer = toggle.isOn;
                toggle.transform.parent.SetSiblingIndex(5);
            }
        }
    }



    [HarmonyPatch(typeof(EngineSounds), "Instantiate")]
    private class GetBike
    {
        public static void Postfix(HoverbikeModel model)
        {
            bikeModel = model;
            Log.Msg($"Bike Model: {model}");
        }
    }

    internal static HoverbikeModel? bikeModel = null;

    internal static int TrySetupUI = 0;
    internal static ASCIILabel RaceText = null;
    internal static ASCIILabel SumText = null;
    internal static GameObject resetObj = null;
    internal static double Now = 0.0;

    internal static Color textColor = new(0.7f, 0.7f, 0.7f, 1.0f);

    internal static double fastestRace = 0.0;
    internal static double fastestSum = 0.0;

    // [HarmonyPatch(typeof(QuantumGame), "OnUpdateDone")]
    [HarmonyPatch(typeof(DeterministicSession), "UpdateSimulationInner")]
    private class Update
    {
        public static void Postfix(DeterministicSession __instance)
        {
            Now = __instance.SimulationTimeElasped;

            // NotRandom();

            if (TrySetupUI > 0)
            {
                if (TrySetupUI == 1)
                {
                    try 
                    {
                        var hud = GameObject.Find("Scoreboard_Scorebox(Clone)").transform;
                        ASCIILabel txt = null;

                        foreach (var child in hud)
                        {
                            if (child.TryCast<Transform>() is Transform tra)
                            {
                                try {
                                    if (tra.name == "NameLabel" && tra.TryGetComponent<ASCIILabel>(out var agh))
                                    {
                                        txt = agh;
                                        Log.Msg("Found a match!");
                                    }
                                } 
                                catch (System.Exception ex)
                                {
                                    Log.Error(ex);
                                }
                            }
                            else
                            {
                                Log.Error("Object not transform");
                            }
                        }

                        if (txt is not null)
                        {
                            var obj = UnityEngine.Object.Instantiate(txt.transform.gameObject, hud);
                            var obj2 = UnityEngine.Object.Instantiate(txt.transform.gameObject, hud);
                            
                            obj.transform.localPosition = new Vector3(170, -48, 0);
                            obj2.transform.localPosition = new Vector3(170, -98, 0);

                            obj.transform.localScale = new Vector3(2.4f, 1.97f, 1.0f);
                            obj2.transform.localScale = new Vector3(2.35f, 1.93f, 1.0f);

                            RaceText = obj.GetComponent<ASCIILabel>();
                            SumText = obj2.GetComponent<ASCIILabel>();

                            RaceText.Text = "00:00.00";
                            SumText.Text = "00:00.00";

                            RaceText.freeColorMode = true;
                            SumText.freeColorMode = true;

                            RaceText.gameObject.SetActive(false);
                            SumText.gameObject.SetActive(false);
                            RaceText.gameObject.SetActive(true);
                            SumText.gameObject.SetActive(true);

                            RaceText.freeColor = textColor;
                            SumText.freeColor = textColor;
                        }
                        else
                        {
                            Log.Msg("Could not match text object :(");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Log.Error(ex);
                    }
                }

                TrySetupUI--;
            }
            

            // if (Beput.Input.GetKeyInt(Beput.KeyCode.R))
            if (resetObj is not null && resetObj.activeSelf)
            {
                Log.Msg("R!!!!!!");

                resetObj.SetActive(false);

                Reset = true;
            }


            if (RaceText is null) return;


            if (Reset)
            {
                RaceTimes.Clear();
                Reset = false;
            }

            var sum = RaceSum();

            SumText.Text = $"{System.TimeSpan.FromSeconds(sum):mm\\:ss\\.ff}";
            SumText.freeColor = TextColor(fastestSum > sum);

            if (RaceStart is double t)
            {
                RaceText.Text = $"{System.TimeSpan.FromSeconds(Now - t):mm\\:ss\\.ff}";
                RaceText.freeColor = textColor;
            }
            else if (RaceTimes.Count >= 1 && RaceTimes[^1] is (double start, double end))
            {
                var lastRaceTime = end - start;
                RaceText.Text = $"{System.TimeSpan.FromSeconds(lastRaceTime):mm\\:ss\\.ff}";

                RaceText.freeColor = TextColor(fastestRace > lastRaceTime);
            }

            RaceText.Resized();
            SumText.Resized();

            RaceText.gameObject.SetActive(false);
            SumText.gameObject.SetActive(false);
            RaceText.gameObject.SetActive(true);
            SumText.gameObject.SetActive(true);
        }
    }

    internal static Color TextColor(bool fast)
    {
        if (fast)
        {
            float sin = Mathf.Sin((float)Now * 3.6f) * 0.375f + 0.62f;
            return new Color(textColor.r * sin, textColor.g + (1.0f - sin) * 0.25f, textColor.b * sin);
        }
        else
            return textColor;
    }

    internal static bool Reset = false;


    // [HarmonyPatch(typeof(UnityEngine.Debug), "Log", [typeof(Il2CppSystem.Object)])]
    // [HarmonyPatch(typeof(UnityEngine.Debug), "Log", [typeof(Il2CppSystem.Object), typeof(UnityEngine.Object)])]
    // [HarmonyPatch(typeof(UnityEngine.Debug), "LogWarning", [typeof(Il2CppSystem.Object)])]
    // [HarmonyPatch(typeof(UnityEngine.Debug), "LogWarning", [typeof(Il2CppSystem.Object), typeof(UnityEngine.Object)])]
    // [HarmonyPatch(typeof(UnityEngine.Debug), "LogError", [typeof(Il2CppSystem.Object)])]
    // [HarmonyPatch(typeof(UnityEngine.Debug), "LogError", [typeof(Il2CppSystem.Object), typeof(UnityEngine.Object)])]

    [HarmonyPatch(typeof(StragglerBoostSystem), "OnInit")]
    private class InitUI
    {
        public static void Postfix() // Straggler boost positions:
        {
            TrySetupUI = 270;
            RaceTimes.Clear();

            fastestRace = 0.0;
            fastestSum = 0.0;
        }
    }

    [HarmonyPatch(typeof(RaceGameModeSystem), "EnterRaceMode")]
    private class EnterRaceMode
    {
        public static void Postfix() // RACE START
        {
            RaceStart = Now;
            Log.Msg("RaceStart set to Now!");
        }
    }

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

    [HarmonyPatch(typeof(SessionRunner), "Shutdown")]
    private class Shutdown
    {
        public static void Postfix() // Shutting down runner
        {
            RaceText = null;
            SumText = null;
            resetObj = null;

            RaceStart = null;
            bikeModel = null;
            RaceTimes.Clear();

            fastestRace = 0.0;
            fastestSum = 0.0;

            Log.Msg($"Game time: {System.TimeSpan.FromSeconds(Now):mm\\:ss\\.ff}");

            // Z:\home\knightragu\Documents\Airframe Ultra Playtest 3\Airframe Ultra Playtest\BepInEx\plugins\AddAirToFrame.dll
            string path = Assembly.GetExecutingAssembly().Location;
            path = path.Remove(path.LastIndexOf('\\') + 1);
            path += folderName;

            string playtimesFile = path + $"\\Playtimes.txt";

            Log.Msg(playtimesFile);

            try
            {
                TryAgain:

                if (Directory.Exists(path))
                {
                    if (File.Exists(playtimesFile))
                    {
                        var times = File.ReadAllLines(playtimesFile);

                        int i;
                        string model;

                        if (bikeModel is not null)
                        {
                            i = (int)bikeModel;
                            model = System.Enum.GetName(bikeModel.Value);
                        }
                        else
                        {
                            i = 4;
                            model = "Nobike";
                        }

                        double totalTime = Now;

                        { // Decide what to write
                            if (double.TryParse(times[i].Split(' ')[0], out var num))
                            {
                                totalTime = System.TimeSpan.FromHours(num).TotalSeconds + Now;
                            }
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

                        times[5] = $"{System.TimeSpan.FromSeconds(sum).TotalHours} // Total Playtime";

                        File.WriteAllLines(playtimesFile, times);
                    }
                    else
                    {
                        string[] times = ["0.0", "0.0", "0.0", "0.0", "0.0", "", "0.0"];

                        int i;
                        string model;

                        if (bikeModel is not null)
                        {
                            i = (int)bikeModel;
                            model = System.Enum.GetName(bikeModel.Value);
                        }
                        else
                        {
                            i = 4;
                            model = "Nobike";
                        }

                        string time = $"{System.TimeSpan.FromSeconds(Now).TotalHours} // ";

                        times[i] = time + model;
                        times[6] = times[i] + "Total Playtime";

                        File.WriteAllLines(playtimesFile, times);
                    }
                }
                else
                {
                    Directory.CreateDirectory(path);

                    goto TryAgain;
                }
            }
            catch (System.Exception)
            {
                
                throw;
            }

            Now = 0.0;

            // UnityEngine.Random
            // Random
        }
    }


    [HarmonyPatch(typeof(BikeRespawnSystem), "SpawnBike")]
    private class DisableAllRacingInhibitors
    {
        public static void Postfix(Frame f, EntityRef playerEntity, PlayerRef playerRef, Transform3D spawnTransform)
        {   
            Log.Msg($"{playerEntity.Index}, {playerEntity}");

            Il2CppSystem.Collections.Generic.List<EntityRef> refs = new();
            f.GetAllEntityRefs(refs);

            if (RemoveEstateDoor.value == true)
            {
                try
                {
                    foreach(var blocker in GameObject.FindObjectsOfType<QPrototypePathBlocker>())
                    {
                        Log.Msg(blocker.name);
    
                        foreach(var r in refs)
                            if (r.Index == int.Parse(blocker.name.Split('.')[1]))
                                f.Destroy(r);
                    }
                }
                catch (System.Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
            

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
                    if (enableTimer)
                    {
                        f.SystemDisable(sys);
                        Log.Msg("Disabled " + type.Name + " | ");
                    }
                    else
                    {
                        f.SystemEnable(type);
                        Log.Msg("Enabled " + type.Name + " | ");
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(ComponentDataBuffer), "Create")]
    private class CreateDataBuf
    {
        public static void Postfix(ComponentDataBuffer __result, Type type)
        {
            
        }
    }
}


// public class RemovePathBlockers : SystemMainThreadFilter<BikeRespawnSystem.Filter>
// {
//     public RemovePathBlockers() {}
//     public RemovePathBlockers(System.IntPtr pointer) {}


//     public override void Update(Frame f)
//     {
//         Il2CppSystem.Collections.Generic.List<EntityRef> refs = new();
//         f.GetAllEntityRefs(refs);

//         foreach(var r in refs)
//         {
//             TimerMod.Log.Msg($"ref: {r.Index}");
//         }

//         try
//         {
//             foreach (var blocker in TimerMod.pathBlockers)
//             {
//                 foreach(var r in refs)
//                     if (r.Index == blocker)
//                         f.Destroy(r);
//             }
//         }
//         catch (System.Exception ex)
//         {
//             TimerMod.Log.Error(ex.ToString());
//         }
//     }
// }
