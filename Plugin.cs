using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System;
using Quantum;
using HarmonyLib.Tools;
using BepInEx;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine.SceneManagement;
using Photon.Deterministic;
using View_Entities;
using Menus;
using TMPro;
using CustomUIRendering_Access;
using Quantum_Game;
using Il2CppSystem.Linq;
using Il2CppSystem.Security.Cryptography;

namespace TimerMod;

[BepInPlugin(guid, "Timer Mod", "0.8.1")]
public class TimerMod : BasePlugin
{
    const string guid = "knightragu.timermod";

    const string folderName = "times";

    internal static new ManualLogSource Log;

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

    public override void Load()
    {
        // Plugin startup logic
        Log = base.Log;
        Log.LogInfo($"Plugin {guid} is loading!");

        try {

            HarmonyFileLog.Enabled = true;

            Harmony.CreateAndPatchAll(typeof(TimerMod));
        }
        catch(Exception ex) {
            Log.LogError(ex);
        }
    }

    internal static Toggle? toggle;

    [HarmonyPatch(typeof(AirframeMainMenu), "Update")]
    [HarmonyPostfix]
    public static void MainMenuUpdate()
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


    [HarmonyPatch(typeof(EngineSounds), "Instantiate")]
    [HarmonyPostfix]
    public static void GetBike(HoverbikeModel model)
    {
        bikeModel = model;
        Log.LogInfo($"Bike Model: {model}");
    }

    internal static HoverbikeModel? bikeModel = null;
    internal static List<int> pathBlockers = [];

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
    [HarmonyPostfix]
    public static void Update(DeterministicSession __instance)
    {
        Now = __instance.FramesAsSeconds(__instance.NextFrame - 1);

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
                                    Log.LogInfo("Found a match!");
                                }
                            } 
                            catch (Exception ex)
                            {
                                Log.LogError(ex);
                            }
                        }
                        else
                        {
                            Log.LogError("Object not transform");
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
                        RaceText._freeColorMode = true;
                        SumText._freeColorMode = true;

                        RaceText.gameObject.active = false;
                        SumText.gameObject.active = false;
                        RaceText.gameObject.active = true;
                        SumText.gameObject.active = true;

                        RaceText.freeColor = textColor;
                        SumText.freeColor = textColor;
                    }
                    else
                    {
                        Log.LogInfo("Could not match text object :(");
                    }
                }
                catch (Exception ex)
                {
                    Log.LogError(ex);
                }
            }

            TrySetupUI--;
        }
        

        // if (Beput.Input.GetKeyInt(Beput.KeyCode.R))
        if (resetObj is not null && resetObj.active)
        {
            Log.LogInfo("R!!!!!!");

            resetObj.active = false;

            Reset = true;
        }


        if (RaceText is null) return;


        if (Reset)
        {
            RaceTimes.Clear();
            Reset = false;
        }

        var sum = RaceSum();

        SumText.Text = $"{TimeSpan.FromSeconds(sum):mm\\:ss\\.ff}";
        SumText.freeColor = TextColor(fastestSum > sum);

        if (RaceStart is double t)
        {
            RaceText.Text = $"{TimeSpan.FromSeconds(Now - t):mm\\:ss\\.ff}";
            RaceText.freeColor = textColor;
        }
        else if (RaceTimes.Count >= 1 && RaceTimes[^1] is (double start, double end))
        {
            var lastRaceTime = end - start;
            RaceText.Text = $"{TimeSpan.FromSeconds(lastRaceTime):mm\\:ss\\.ff}";

            RaceText.freeColor = TextColor(fastestRace > lastRaceTime);
        }

        RaceText.Resized();
        SumText.Resized();

        RaceText.gameObject.active = false;
        SumText.gameObject.active = false;
        RaceText.gameObject.active = true;
        SumText.gameObject.active = true;
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

    // [HarmonyPatch(typeof(Debug), "Log", [typeof(string)])]
    // [HarmonyPatch(typeof(Debug), "LogInfo", [typeof(string)])]

    [HarmonyPatch(typeof(UnityEngine.Debug), "Log", [typeof(Il2CppSystem.Object)])]
    [HarmonyPatch(typeof(UnityEngine.Debug), "Log", [typeof(Il2CppSystem.Object), typeof(UnityEngine.Object)])]
    [HarmonyPatch(typeof(UnityEngine.Debug), "LogWarning", [typeof(Il2CppSystem.Object)])]
    [HarmonyPatch(typeof(UnityEngine.Debug), "LogWarning", [typeof(Il2CppSystem.Object), typeof(UnityEngine.Object)])]
    [HarmonyPatch(typeof(UnityEngine.Debug), "LogError", [typeof(Il2CppSystem.Object)])]
    [HarmonyPatch(typeof(UnityEngine.Debug), "LogError", [typeof(Il2CppSystem.Object), typeof(UnityEngine.Object)])]
    [HarmonyPostfix]
    public static void OnLog(Il2CppSystem.Object message)
    {
        var msg = message.ToString();
        // BikeRespawnSystem.debugDraw = true;

        // NotRandom();

        if (!enableTimer) return;

        if (msg.Contains("Straggler boost positions:"))
        {
            TrySetupUI = 270;
            RaceTimes.Clear();

            fastestRace = 0.0;
            fastestSum = 0.0;
        } 
        else if (msg.Contains("RACE MODE"))
        {
            RaceStart = Now;
            Log.LogInfo("RaceStart set to Now!");

            foreach(var blocker in GameObject.FindObjectsOfType<QPrototypePathBlocker>())
            {
                Log.LogInfo(blocker.name);

                pathBlockers.Add(int.Parse(blocker.name.Split('.')[1]));
            }
        }
        else if (msg.Contains("crossed the finish line"))
        {
            if (RaceStart is double t)
            {
                RaceTimes.Add((t, Now));

                Log.LogInfo($"Race time: {TimeSpan.FromSeconds(Now - t):mm\\:ss\\.ff}, Total game time: {TimeSpan.FromSeconds(Now):mm\\:ss\\.ff}");
            }
            else
            {
                Log.LogInfo("RaceStart is null..?");
            }

            var (start, end) = RaceTimes[^1];

            // Z:\home\knightragu\Documents\Airframe Ultra Playtest 3\Airframe Ultra Playtest\BepInEx\plugins\AddAirToFrame.dll
            string path = Assembly.GetExecutingAssembly().Location;
            path = path.Remove(path.LastIndexOf('\\') + 1);
            path += folderName;

            string model;

            if (bikeModel is not null)
                model = Enum.GetName(bikeModel.Value);
            else
                model = "Nobike";

            string mapFile = path + $"\\{SceneManager.GetActiveScene().name} - {model}.txt";

            Log.LogInfo(mapFile);

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
                        if (i >= times.Length) Array.Resize(ref times, i + 1);

                        for (int f = 0; f < times.Length; f++)
                            if (times[f] is null) times[f] = "";
                    }

                    { // Decide what to write
                        var pair = times[i].Split("|");

                        if (pair.Length == 2)
                        {
                            if (double.TryParse(pair[0], out var num))
                            {
                                raceTime = Math.Min(raceTime, num);
                                fastestRace = num;
                            }
                            

                            if (double.TryParse(pair[1], out var num2))
                            {
                                sumTime = Math.Min(sumTime, num2);
                                fastestSum = num2;
                            }
                        }
                    }

                    times[i] = $"{raceTime}|{sumTime}";

                    Log.LogInfo($"{raceTime}|{sumTime}");

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
        else if (msg.Contains("Shutting down runner"))
        {
            RaceText = null;
            SumText = null;
            resetObj = null;

            RaceStart = null;
            bikeModel = null;
            RaceTimes.Clear();

            fastestRace = 0.0;
            fastestSum = 0.0;

            Log.LogInfo($"Game time: {TimeSpan.FromSeconds(Now):mm\\:ss\\.ff}");

            // Z:\home\knightragu\Documents\Airframe Ultra Playtest 3\Airframe Ultra Playtest\BepInEx\plugins\AddAirToFrame.dll
            string path = Assembly.GetExecutingAssembly().Location;
            path = path.Remove(path.LastIndexOf('\\') + 1);
            path += folderName;

            string playtimesFile = path + $"\\Playtimes.txt";

            Log.LogInfo(playtimesFile);

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
                            model = Enum.GetName(bikeModel.Value);
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
                                totalTime = TimeSpan.FromHours(num).TotalSeconds + Now;
                            }
                        }
                        

                        times[i] = $"{TimeSpan.FromSeconds(totalTime).TotalHours} // {model}";

                        double sum = 0.0;

                        for (int f = 0; f < 4; f++)
                        {
                            if (double.TryParse(times[f].Split(' ')[0], out var num))
                            {
                                sum += TimeSpan.FromHours(num).TotalSeconds;
                            }
                        }

                        times[5] = $"{TimeSpan.FromSeconds(sum).TotalHours} // Total Playtime";

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
                            model = Enum.GetName(bikeModel.Value);
                        }
                        else
                        {
                            i = 4;
                            model = "Nobike";
                        }

                        string time = $"{TimeSpan.FromSeconds(Now).TotalHours} // ";

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
            catch (Exception)
            {
                
                throw;
            }

            Now = 0.0;

            // UnityEngine.Random
            // Random
        }
        // else if (msg.Contains("OperationResponse"))
        // {
        //     // NotRandom();
        // }
    }


    [HarmonyPatch(typeof(BikeRespawnSystem), "SpawnBike")]
    [HarmonyPostfix]
    public static void DisableAllRacingInhibitors(Frame f, EntityRef playerEntity, PlayerRef playerRef, Transform3D spawnTransform)
    {   
        Log.LogInfo($"{playerEntity.Index}, {playerEntity.Raw}, {playerEntity.ToString()}");

        Il2CppSystem.Collections.Generic.List<EntityRef> refs = new();
        f.GetAllEntityRefs(refs);

        try
        {
            foreach (var blocker in pathBlockers)
            {
                foreach(var r in refs)
                    if (r.Index == blocker)
                        f.Destroy(r);
            }
        }
        catch (System.Exception ex)
        {
            Log.LogError(ex.ToString());
        }

        // if (!f.SystemsAll.Contains(new RemovePathBlockers()))
        // {
        //     var system = new RemovePathBlockers();
        //     Log.LogInfo("Adding: " + system.ToString());
        //     f.SystemsAll.AddItem(system);
        //     f._systemsAll.AddItem(system);
        // }

        foreach(var sys in f.SystemsAll)
        {
            var name = sys.GetIl2CppType().Name;

            var enuu = sys.ChildSystems.GetEnumerator();
            for (int i = 0; i < sys.ChildSystems.Count(); i++)
            {
                var littleSys = enuu.Current;
                var littleName = littleSys.GetIl2CppType().Name;

                Log.LogInfo("Child: " + littleName);
            }

            if (
                name == "PoliceCoordinatorSystem"
                || name == "PoliceHelicopterSystem"
                || name == "PoliceGunmanSystem"

                || name.Contains("PickupSpawnSystem")
                || name.Contains("SpecialPickupSpawnSystem")
            ) {
                if (enableTimer)
                {
                    f.SystemDisable(sys);
                    Log.LogInfo("Disabled " + name + " | " + sys._startEnabled);
                } else
                {
                    f.SystemEnable(sys.GetIl2CppType());
                    Log.LogInfo("Enabled " + name + " | " + sys._startEnabled);
                }
            } else
            {
                // Log.LogInfo(name);
            }
        }
    }



    [HarmonyPatch(typeof(Il2CppSystem.Random), "GenerateSeed")]
    [HarmonyPostfix]
    public static void RandGen(Il2CppSystem.Random __instance)
    {
        __instance._inext = 0;
        __instance._inextp = 0;

        for (int i = 0; i < __instance._seedArray.Length; i++)
        {
            __instance._seedArray[i] = 0;
        }
        
        // Log.LogInfo("RANDOM!!! Gen");
    }

    [HarmonyPatch(typeof(Il2CppSystem.Random), "GenerateGlobalSeed")]
    [HarmonyPostfix]
    public static void RandGenGlob(Il2CppSystem.Random __instance)
    {
        __instance._inext = 0;
        __instance._inextp = 0;

        for (int i = 0; i < __instance._seedArray.Length; i++)
        {
            __instance._seedArray[i] = 0;
        }

        // Log.LogInfo("RANDOM!!! GenGlob");
    } 

    [HarmonyPatch(typeof(Il2CppSystem.Random), "GetSampleForLargeRange")]
    [HarmonyPostfix]
    public static void RandGetSampLrgRng(Il2CppSystem.Random __instance)
    {
        __instance._inext = 0;
        __instance._inextp = 0;

        for (int i = 0; i < __instance._seedArray.Length; i++)
        {
            __instance._seedArray[i] = 0;
        }

        // Log.LogInfo("RANDOM!!! GetSampLrgRng");
    } 

    [HarmonyPatch(typeof(Il2CppSystem.Random), "InternalSample")]
    [HarmonyPostfix]
    public static void RandInternalSample(Il2CppSystem.Random __instance)
    {
        __instance._inext = 0;
        __instance._inextp = 0;

        for (int i = 0; i < __instance._seedArray.Length; i++)
        {
            __instance._seedArray[i] = 0;
        }

        // Log.LogInfo("RANDOM!!! InternalSample");
    } 

    [HarmonyPatch(typeof(Il2CppSystem.Random), "Next", [typeof(int), typeof(int)])]
    [HarmonyPostfix]
    public static void RandNext1(Il2CppSystem.Random __instance)
    {
        __instance._inext = 0;
        __instance._inextp = 0;

        for (int i = 0; i < __instance._seedArray.Length; i++)
        {
            __instance._seedArray[i] = 0;
        }

        // Log.LogInfo("RANDOM!!! Next1");
    } 

    [HarmonyPatch(typeof(Il2CppSystem.Random), "Next", [typeof(int)])]
    [HarmonyPostfix]
    public static void RandNext2(Il2CppSystem.Random __instance)
    {
        __instance._inext = 0;
        __instance._inextp = 0;

        for (int i = 0; i < __instance._seedArray.Length; i++)
        {
            __instance._seedArray[i] = 0;
        }

        // Log.LogInfo("RANDOM!!! Next2");
    } 

    [HarmonyPatch(typeof(Il2CppSystem.Random), "Next", [])]
    [HarmonyPostfix]
    public static void RandNext3(Il2CppSystem.Random __instance)
    {
        __instance._inext = 0;
        __instance._inextp = 0;

        for (int i = 0; i < __instance._seedArray.Length; i++)
        {
            __instance._seedArray[i] = 0;
        }

        // Log.LogInfo("RANDOM!!! Next3");
    } 

    [HarmonyPatch(typeof(Il2CppSystem.Random), "Sample")]
    [HarmonyPostfix]
    public static void RandSample()
    {
        // Log.LogInfo("RANDOM!!! Sample");
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
//             TimerMod.Log.LogInfo($"ref: {r.Index}");
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
//             TimerMod.Log.LogError(ex.ToString());
//         }
//     }
// }
