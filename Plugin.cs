// using BepInEx;
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
using Quantum_HoverBikeShared;
using Quantum.Prototypes;
using View_Entities;
using static Quantum.QuantumGame;
using Menus;
using TMPro;
// using Il2CppSystem;

namespace TimerMod;

[BepInPlugin(guid, "Timer Mod", "0.8.1")]
public class TimerMod : BasePlugin
{
    const string guid = "knightragu.timermod";

    const string folderName = "times";

    // internal const int oftenity = 40;
    // internal static int countDown = 0;

    internal static new ManualLogSource Log;

    internal static bool enableTimer = true;

    // internal static DateTime GameStart = DateTime.Now;
    internal static double? RaceStart = null;
    // internal static DateTime RaceEnd = DateTime.Now;
    internal static List<(double start, double end)> RaceTimes = [];

    internal static double RaceSum()
    {
        double sum = 0.0;

        foreach (var (start, end) in RaceTimes)
            sum += end - start;

        return sum;
    }

    // internal static Harmony harmony;

    // private static readonly MethodInfo GetBoosts =
    //     typeof(HoverBikePrototype).GetMethod("get_boosts", BindingFlags.Public | BindingFlags.Instance);

    // private static readonly MethodInfo GetBoostsPatch =
    //     typeof(TimerMod).GetMethod("Boost", BindingFlags.Static | BindingFlags.Public);
    
    // private static readonly MethodInfo QuitPatch =
    //     typeof(TimerMod).GetMethod("Quitting", BindingFlags.Static | BindingFlags.Public);

    public override void Load()
    {
        // Plugin startup logic
        Log = base.Log;
        Log.LogInfo($"Plugin {guid} is loading!");

        try {

            HarmonyFileLog.Enabled = true;

            Harmony.CreateAndPatchAll(typeof(TimerMod));

            // harmony = new("test_harmony_idk");

            // if (GetBoosts == null)
            //     Log.LogError("Unable to patch HoverBikePrototype.get_boosts - Method not found!");
            // else
            //     harmony.Patch(GetBoosts, null, new HarmonyMethod(GetBoostsPatch));
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

    // [HarmonyPatch(typeof(DeterministicSession), "Destroy")]
    // [HarmonyPrefix]
    public static void Quitting()
    {
        Log.LogInfo("Exiting!");
    }

    // [HarmonyPatch(typeof(QuantumGame), MethodType.Constructor, [typeof(System.IntPtr)], [ArgumentType.Normal])]
    // [HarmonyPatch(typeof(QuantumGame), MethodType.Constructor, [typeof(QuantumGameStartParameters)], [ArgumentType.Ref])]
    // [HarmonyPatch(typeof(QuantumGame), MethodType.Constructor, [typeof(StartParameters)], [ArgumentType.Normal])]
    // [HarmonyPatch(typeof(QuantumGame), MethodType.Constructor, [typeof(IResourceManager), typeof(IAssetSerializer), typeof(ICallbackDispatcher), typeof(IEventDispatcher)], [ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal])]
    
    // [HarmonyPatch(typeof(QuantumGame), "CreateFrame")]
    // [HarmonyPatch(typeof(QuantumGame), "OnGameStart")]
    // [HarmonyPrefix]
    public static void GameStart()
    {
        Log.LogInfo("ASDAGA");
        NotRandom();
    }


    [HarmonyPatch(typeof(EngineSounds), "Instantiate")]
    [HarmonyPostfix]
    public static void GetBike(HoverbikeModel model)
    {
        bikeModel = model;
        Log.LogInfo($"Bike Model: {model}");
    }

    internal static HoverbikeModel? bikeModel = null;

    internal static int TrySetupUI = 0;
    internal static Text RaceText = null;
    internal static Text SumText = null;
    internal static GameObject resetObj = null;
    internal static double Now = 0.0;

    internal static Color textColor = Color.white;

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
                    var hud = GameObject.Find("HUD_Canvas(Clone)").transform;
                    Text txt = null;

                    foreach (var child in hud)
                    {
                        if (child.TryCast<Transform>() is Transform tra)
                        {
                            try {
                                if (tra.TryGetComponent<Text>(out var agh) && agh.text.Contains('$'))
                                {
                                    Log.LogInfo("Found a match!");
                                    txt = agh;
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
                        
                        obj.transform.localPosition = new Vector3(331, 45, 0);
                        obj2.transform.localPosition = new Vector3(331, 0, 0);

                        RaceText = obj.GetComponent<Text>();
                        SumText = obj2.GetComponent<Text>();

                        RaceText.text = "00:00.00";
                        SumText.text = "00:00.00";

                        textColor = RaceText.color;
                        Log.LogInfo($"Saved: {textColor}");
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

        SumText.text = $"{TimeSpan.FromSeconds(sum):mm\\:ss\\.ff}";
        SumText.color = TextColor(fastestSum > sum);

        if (RaceStart is double t)
        {
            RaceText.text = $"{TimeSpan.FromSeconds(Now - t):mm\\:ss\\.ff}";
            RaceText.color = textColor;
        }
        else if (RaceTimes.Count >= 1 && RaceTimes[^1] is (double start, double end))
        {
            var lastRaceTime = end - start;
            RaceText.text = $"{TimeSpan.FromSeconds(lastRaceTime):mm\\:ss\\.ff}";

            RaceText.color = TextColor(fastestRace > lastRaceTime);
        }
    }

    internal static Color TextColor(bool fast)
    {
        if (fast)
        {
            float sin = Mathf.Sin((float)Now * 3.6f) * 0.375f + 0.62f;
            return new Color(textColor.r * sin, textColor.g, textColor.b);
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

    public static void NotRandom()
    {
        var old = UnityEngine.Random.seed;

        int seed = 10;
        // UnityEngine.Random.State state = new()
        // {
        //     s0 = seed,
        //     s1 = seed,
        //     s2 = seed,
        //     s3 = seed
        // };

        // UnityEngine.Random.set_state_Injected(ref state);
        if (old != 10) UnityEngine.Random.InitState(seed);

        Log.LogInfo($"Seed was {old}, but is now {UnityEngine.Random.seed}!");
    }

    /* [HarmonyPatch(typeof(UnityEngine.Random), "Range", [typeof(int), typeof(int)])]
    [HarmonyPatch(typeof(UnityEngine.Random), "RandomRange", [typeof(int), typeof(int)])]
    [HarmonyPatch(typeof(UnityEngine.Random), "RandomRangeInt", [typeof(int), typeof(int)])]
    [HarmonyPostfix]
    public static void RandInt(int __result)
    {
        Log.LogInfo($"RandInt: {__result}");
    }

    [HarmonyPatch(typeof(UnityEngine.Random), "Range", [typeof(float), typeof(float)])]
    [HarmonyPatch(typeof(UnityEngine.Random), "RandomRange", [typeof(float), typeof(float)])]
    [HarmonyPostfix]
    public static void RandFloat(float __result)
    {
        Log.LogInfo($"RandFloat: {__result}");
    } */

    // [HarmonyPatch(typeof(HoverBikeShared), "LeaningBikeRotation", [typeof(Transform3D), typeof(HoverBike)], [ArgumentType.Ref, ArgumentType.Ref])]
    // [HarmonyPatch(typeof(HoverBikeShared), "LeaningBikeRotation", [typeof(Transform3D), typeof(HoverBike)], [ArgumentType.Pointer, ArgumentType.Pointer])]
    // [HarmonyPostfix]
    public static void Boosts(ref HoverBike hoverBike/* , FPQuaternion __result */)
    {
        hoverBike.boostCounter = new FP(10000);
        hoverBike.boosts = 10000;

        // Log.LogInfo(__result.AsEuler.ToString());
    }

    public static void MapScalePostfix(ref int __result)
    {
        Log.LogInfo($"get_boosts: {__result}");
        __result = 10000;
    }

    // [HarmonyPatch(typeof(BikeRespawnSystem), "SpawnBike")]
    // [HarmonyPatch(typeof(BikeRespawnSystem), "TryFindDynamicBikeRespawnPos")]
    // [HarmonyPatch(typeof(BikeRespawnSystem), "GetNoBikeZoneBikeSpawnpos")]
    // [HarmonyPatch(typeof(BikeRespawnSystem), "DespawnAllUnoccupiedBikesAndRespawnNewOnes")]
    // [HarmonyPatch(typeof(BikeRespawnSystem), "TrySpawnReplacementVehicle", [typeof(Frame), typeof(EntityRef)])]
    // [HarmonyPatch(typeof(BikeRespawnSystem), "TrySpawnReplacementVehicle", [typeof(Frame), typeof(RespawnSystem.Filter)], [ArgumentType.Normal, ArgumentType.Ref])]

    // [HarmonyPatch(typeof(HoverBikeSystem), "Locomote")]

    // [HarmonyPrefix]
    public static void NoBike(Frame f)
    {
        // List<EntityRef> l = new();
        // f.GetAllEntityRefs(l);

        // for (int i = l.Count - 1; i >= 0; i--)
        // {
            
        // }

        foreach (var c in f.GetComponentIterator<HoverBike>())
        {
            var comp = c.Component;
            comp.boosts = 999;
            
            c.Component = comp;
        }        

        // return false;
    }
}
