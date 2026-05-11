using HarmonyLib;
using Il2CppMenus;
using Il2CppQuantum;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

namespace TimerMod;

public partial class Timer
{
    internal static Toggle? UiToggle;
    
    internal static Retry? RetryInfo = null;
    internal static RuntimeConfig? LastConfig = null;
}

[HarmonyPatch(typeof(AirframeMainMenu), nameof(AirframeMainMenu.Update))]
static class AirframeMainMenu_Update_Patch
{
    public static void Postfix()
    {
        if (Timer.UiToggle == null)
        {
            if (GameObject.Find("SettingsScrollview") is GameObject gameObj)
            {
                var option = gameObj.transform.GetChild(0).GetChild(0).GetChild(3).gameObject;
                var timerOption = GameObject.Instantiate(option);

                timerOption.transform.SetParent(option.transform.parent, false);
                timerOption.transform.localScale = Vector3.one;

                if (timerOption.transform.GetChild(0)?.GetComponent<TextMeshProUGUI>() is TextMeshProUGUI text)
                    text.text = "Timer";

                if (timerOption.GetComponentInChildren<Toggle>() is Toggle tog)
                    Timer.UiToggle = tog;
            }
        }
        else
        {
            Timer.enabled = Timer.UiToggle.isOn;
            Timer.UiToggle.transform.parent.SetSiblingIndex(5);
        }


        if (Timer.RetryInfo is not null)
        {
            foreach (var button in GameObject.FindObjectsOfType<Button>())
            {
                if (button is Button o)
                {
                    if (o.name == "PlayButton" || o.name == "PlayOfflineButton")
                    {
                        o.gameObject.SetActive(true);
                        o.enabled = true;                        
                        o.Press();
                        o.m_OnClick.Invoke();
                        o.onClick.Invoke();
                    }
                    
                } else Timer.Log.Error("object null");
            }
        }
    }
}

public class Retry()
{
    public RetryMethod Type = RetryMethod.SameSeed;
    public Quickstop[] QuickstopToggles = [
        Quickstop.Ignore,
        Quickstop.Ignore,
        Quickstop.Enable,
        Quickstop.Ignore,
        Quickstop.Enable,
        Quickstop.Ignore,
        Quickstop.Ignore,
        Quickstop.Ignore
    ];

    internal int Seed = 0;
}

public enum RetryMethod {
    SameSeed,
    RandomSeed,
    InfiniteRandomSeed,
    RandomQuickstopSeed,
}

public enum Quickstop {
    Enable = 1,
    Disable = 0,
    Ignore = -1,
}