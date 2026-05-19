using System.Collections.Generic;
using UnityEngine;
using Il2CppCustomUIRendering_Access;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace TimerMod;

public class LabelManager
{
    public List<TimerLabel> Labels = [];

    private GameObject? timerUI = null;
    private string initializedIn = "";
    // private bool Initialized(out GameObject ui) =>  && ;

    private SkewContainer skew = null;
    private int howMany = 0;
    

    public void Initialize(int labels)
    {
        Labels = [];
        howMany = labels;
        timerUI = null;

        initializedIn = SceneManager.GetActiveScene().name;
    }

    public bool TryGetLabel(int labelIndex, out TimerLabel label)
    {
        label = null;
        if (labelIndex < 0 || labelIndex >= this.Labels.Count) return false;

        label = Labels[labelIndex];
        return true;
    }

    public void Update()
    {
        if (initializedIn != SceneManager.GetActiveScene().name) {
            timerUI = null;
            return;
        }

        if (timerUI is null) {
            if (Timer.CurrentRace.totalElapsedTime >= 12)
                CreateLabels();
            
            return;
        }

        // ui.transform.localPosition = new(-272.3645f, 257.8108f, 0.0f);

        foreach (var label in Labels)
            label.Update();
        
        if (this.skew is SkewContainer s)
        {
            s.pinch = new(1.4f, -0.5f);

            var time = 1.0 - ((Timer.CurrentRace.TotalElapsedSeconds - 1.0) * 2.0);
            s.Wipe = (float)Math.Max(time * time * time, 0.0);
        }
    }

    private void CreateLabels()
    {
        Labels = [];
        SetupUI();

        for (int i = 0; i < howMany; i++)
            CreateNewLabel();
    }

    private void SetupUI()
    {
        try
        {
            Transform timerUITransform = null;

            foreach (var container in GameObject.FindObjectsOfType<SkewContainer>())
                if (container.name == "ScoreBoard")
                {
                    timerUI = container.gameObject;
                    timerUITransform = container.gameObject.transform;
                }

            // Transform timerUI = GameObject.Instantiate(original).transform;
            // timerUI.name = "TimerUI";

            // timerUI.SetParent(original.transform.parent);

            // original.transform.position += Vector3.up * 100.0f;

            timerUITransform.localPosition = new(-272.3645f, 257.8108f, 0.0f);
            timerUITransform.localEulerAngles = new(0.0f, 0.0f, 0.0f);
            timerUITransform.localScale = new(1.0f, 1.0f, 1.0f);

            this.skew = timerUITransform.GetComponent<SkewContainer>();

            this.skew.pinch = new(1.4f, -0.5f);
            this.skew.rotation = new(-7.0f, 0.0f);
            this.skew.skew = new(0.0f, 0.0f);

            ASCIILabel txt = null;

            timerUITransform.GetChild(0).GetComponent<Image>().enabled = false;
            timerUITransform.GetChild(1).GetComponent<RawImage>().enabled = false;

            if (timerUITransform.GetChild(2) is var c1)
            {
                c1.GetComponent<ShadowRect>().enabled = false;
                c1.GetChild(0).GetComponent<RawImage>().enabled = false;

                if (c1.GetChild(1) is var c2)
                {
                    c2.GetComponent<Image>().enabled = false;
                    c2.GetChild(1).GetComponent<RawImage>().enabled = false;

                    if (c2.GetChild(0) is var c3)
                    {
                        if (c3.TryGetComponent<ASCIILabel>(out var agh))
                            txt = agh;
                    }
                }
            }

            if (txt is not null)
            {
                txt.gameObject.SetActive(false);
                txt = GameObject.Instantiate(txt.gameObject).GetComponent<ASCIILabel>();
                txt.transform.SetParent(timerUITransform);

                txt.transform.localPosition = new Vector3(-30, 0, 0);
                txt.transform.localEulerAngles = new Vector3(0, 0, 0);
                txt.transform.localScale = new Vector3(3, 3, 3);

                txt.Text = "00:00.00";
                txt.freeColorMode = true;
                txt.freeColor = TimerLabel.BaseColor;
                txt.gameObject.SetActive(false);

                Labels.Add(new TimerLabel(txt, null));
            }
            else
            {
                Timer.Log.Error("Could not match text object :(");
            }
        }
        catch (System.Exception ex)
        {
            Timer.Log.Error(ex);
        }
    }

    private bool CreateNewLabel()
    {
        try
        {
            if (Labels.Count > 0 && timerUI is not null)
            {
                var txt = UnityEngine.Object.Instantiate(Labels[0].Primary.gameObject).GetComponent<ASCIILabel>();

                txt = GameObject.Instantiate(txt.gameObject).GetComponent<ASCIILabel>();
                txt.transform.SetParent(timerUI.transform);

                txt.transform.localPosition = new Vector3(-30, Labels.Count * -80, 0);
                txt.transform.localEulerAngles = new Vector3(0, 0, 0);
                txt.transform.localScale = new Vector3(3, 3, 3);

                txt.Text = "00:00.00";
                txt.freeColorMode = true;
                txt.freeColor = TimerLabel.BaseColor;
                txt.gameObject.SetActive(false);
                txt.gameObject.SetActive(true);

                Labels.Add(new TimerLabel(txt, null));

                return true;
            }
            else
            {
                Timer.Log.Error("Could not match text object :(");

                return false;
            }
        }
        catch (System.Exception ex)
        {
            Timer.Log.Error(ex);
            return false;
        }
    }
}


public class TimerLabel(ASCIILabel primaryLabel, ASCIILabel secondaryLabel)
{
    internal static Color BaseColor => new(0.7f, 0.7f, 0.7f, 1.0f);

    internal readonly ASCIILabel Primary = primaryLabel;
    internal readonly ASCIILabel Secondary = secondaryLabel;

    public LabelColor PrimaryColor = LabelColor.White;
    public LabelColor SecondaryColor = LabelColor.White;

    private double animationStart = 0.0;
    private float AnimationTime => (float)(Timer.CurrentRace.TotalElapsedSeconds - animationStart);
    private LabelAnimation Animation = LabelAnimation.None;
    private LabelColor AnimationColor = LabelColor.White;

    public long PrimaryText {
        set => Primary.Text = $"{System.TimeSpan.FromSeconds((double)value / 45.0):mm\\:ss\\.ff}";
    }

    public long SecondaryText {
        set {
            if (value >= 0)
                Secondary.Text = $"+{System.TimeSpan.FromSeconds((double)value / 45.0):ss\\.ff}";
            else
                Secondary.Text = $"-{System.TimeSpan.FromSeconds((double)value / 45.0):ss\\.ff}";
        }
    }

    public void PlayAnimation(LabelAnimation animation, LabelColor? animationColor = null)
    {
        Animation = animation;
        if (animationColor.HasValue)
            AnimationColor = animationColor.Value;
        
        animationStart = Timer.CurrentRace.TotalElapsedSeconds;
    }

    internal static Color Color(LabelColor color)
    {
        // float sin = Mathf.Sin((float)Timer.currentRace?.TotalElapsedSeconds * 3.6f) * 0.375f + 0.625f;
        
        return color switch
        {
            LabelColor.White => BaseColor,
            LabelColor.Red => new Color(BaseColor.r + 0.1875f, BaseColor.g * 0.25f, BaseColor.b * 0.25f),
            LabelColor.Green => new Color(BaseColor.r * 0.25f, BaseColor.g + 0.1875f, BaseColor.b * 0.25f),
            LabelColor.Blue => new Color(BaseColor.r * 0.25f, BaseColor.g * 0.25f, BaseColor.b + 0.1875f),
            LabelColor.Gold => new Color(BaseColor.r + 0.2075f, BaseColor.g + 0.1375f, BaseColor.b * 0.25f),
            _ => BaseColor,
        };
    }

    public void Update()
    {
        switch (Animation)
        {
            case LabelAnimation.None:
                Primary.freeColor = Color(PrimaryColor);
            break;

            case LabelAnimation.SlowPulse:
                float sin = Mathf.Sin(AnimationTime * 3.6f) * 0.5f + 0.5f;

                Primary.freeColor = UnityEngine.Color.Lerp(Color(PrimaryColor), Color(AnimationColor), sin);
            break;

            case LabelAnimation.QuickFade:
                Primary.freeColor = UnityEngine.Color.Lerp(Color(PrimaryColor), Color(AnimationColor), AnimationTime * 2.0f);
            break;

            case LabelAnimation.SlowFade:
                Primary.freeColor = UnityEngine.Color.Lerp(Color(PrimaryColor), Color(AnimationColor), AnimationTime / 2.0f);
            break;
        }

        Primary.Resized();
        // Secondary.Resized();

        Primary.gameObject.SetActive(false);
        Primary.gameObject.SetActive(true);
        // Secondary.gameObject.SetActive(false);
        // Secondary.gameObject.SetActive(true);
    }
}


public enum LabelAnimation
{
    None,
    SlowPulse,
    QuickFade,
    SlowFade,
}

public enum LabelColor
{
    White,
    Red,
    Green,
    Blue,
    Gold,
}
