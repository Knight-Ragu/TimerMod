using HarmonyLib;
using Il2CppMenus;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TimerMod;

public partial class Timer
{
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
}