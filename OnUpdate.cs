using HarmonyLib;
using Il2CppCustomUIRendering_Access;
using Il2CppInput;
using Il2CppPhoton.Deterministic;
using Il2CppQuantum;
using Il2CppQuantum_HoverBike;
using UnityEngine;

namespace TimerMod;

public partial class Timer
{
    internal static ASCIILabel RaceText = null;
    internal static ASCIILabel SumText = null;
    internal static Color textColor = new(0.7f, 0.7f, 0.7f, 1.0f);
    internal static bool Reset = false;
    

    // [HarmonyPatch(typeof(QuantumGame), "OnUpdateDone")]
    [HarmonyPatch(typeof(DeterministicSession), "UpdateSimulationInner")]
    private class Update
    {
        public static void Postfix(DeterministicSession __instance)
        {
            Now = __instance.SimulationTimeElasped;

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

    // [HarmonyPatch("Update")]
    // private class Updateee
    // {
    //     public static void Postfix(Frame f)
    //     {
    //         // Log.Msg("upaejisjda");

    //         var fltr = f.Filter<Transform3D, Humanoid>();

    //         unsafe {
    //             for (int i = 0; i < fltr._t1->Count; i++)
    //             {
    //                 if (fltr.Next(out var entity, out var transform, out var player))
    //                 {
    //                     Log.Msg($"transform: {transform.Position}, {player.mode}");
    //                     transform.Position += FPVector3.Up;
    //                 }
    //             }
    //         }
    //     }
    // }
}