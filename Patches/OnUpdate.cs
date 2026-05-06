using HarmonyLib;
using Il2CppCustomUIRendering_Access;
using Il2CppPhoton.Deterministic;
using UnityEngine;

namespace TimerMod;

public partial class Timer
{
    internal static ASCIILabel RaceText = null;
    internal static ASCIILabel SumText = null;
    internal static Color TextBaseColor => new(0.7f, 0.7f, 0.7f, 1.0f);
    internal static int TrySetupUI = 0;
}

// [HarmonyPatch(typeof(QuantumGame), "OnUpdateDone")]
[HarmonyPatch(typeof(DeterministicSession), nameof(DeterministicSession.UpdateSimulationInner))]
class DeterministicSession_UpdateSimulationInner_Patch
{
    public static void Postfix(DeterministicSession __instance)
    {
        Timer.Now = __instance.SimulationTimeElasped;

        if (Timer.TrySetupUI > 0)
        {
            if (Timer.TrySetupUI == 1)
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
                                    Timer.Log.Msg("Found a match!");
                                }
                            } 
                            catch (System.Exception ex)
                            {
                                Timer.Log.Error(ex);
                            }
                        }
                        else
                        {
                            Timer.Log.Error("Object not transform");
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

                        Timer.RaceText = obj.GetComponent<ASCIILabel>();
                        Timer.SumText = obj2.GetComponent<ASCIILabel>();

                        Timer.RaceText.Text = "00:00.00";
                        Timer.SumText.Text = "00:00.00";

                        Timer.RaceText.freeColorMode = true;
                        Timer.SumText.freeColorMode = true;

                        Timer.RaceText.gameObject.SetActive(false);
                        Timer.SumText.gameObject.SetActive(false);
                        Timer.RaceText.gameObject.SetActive(true);
                        Timer.SumText.gameObject.SetActive(true);

                        Timer.RaceText.freeColor = Timer.TextBaseColor;
                        Timer.SumText.freeColor = Timer.TextBaseColor;
                    }
                    else
                    {
                        Timer.Log.Msg("Could not match text object :(");
                    }
                }
                catch (System.Exception ex)
                {
                    Timer.Log.Error(ex);
                }
            }

            Timer.TrySetupUI--;
        }


        if (Timer.RaceText is null) return;



        Timer.SumText.Text = $"{System.TimeSpan.FromSeconds(Timer.SumRaceTime()):mm\\:ss\\.ff}";
        Timer.SumText.freeColor = Timer.TextColor(Timer.wasFastestRaceSum);

        if (Timer.RaceStart is double t)
        {
            Timer.RaceText.Text = $"{System.TimeSpan.FromSeconds(Timer.Now - t):mm\\:ss\\.ff}";
            Timer.RaceText.freeColor = Timer.TextBaseColor;
        }
        else if (Timer.SprintTimes.Count >= 1 && Timer.SprintTimes[^1] is (double start, double end))
        {
            var lastRaceTime = end - start;
            Timer.RaceText.Text = $"{System.TimeSpan.FromSeconds(lastRaceTime):mm\\:ss\\.ff}";

            Timer.RaceText.freeColor = Timer.TextColor(Timer.wasFastestSprint);
        }

        Timer.RaceText.Resized();
        Timer.SumText.Resized();

        Timer.RaceText.gameObject.SetActive(false);
        Timer.SumText.gameObject.SetActive(false);
        Timer.RaceText.gameObject.SetActive(true);
        Timer.SumText.gameObject.SetActive(true);
    }
}