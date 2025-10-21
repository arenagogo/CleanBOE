using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeartRateResultAnalyzer : MonoBehaviour
{

    // public List<int> heartRateData = new List<int>();
    public Image iconTiitle;
    public TextMeshProUGUI textTitle;
    public RectTransform icontiitleAnim;
    public RectTransform tiitleTextAnim;

    public Sprite[] iconEmot;

    void Start()
    {
        // SetAnim();
    }
    // public string test;

    public void SetTittle(List<int> heartRateData)
    {
        if (GlobalVariable.gamemode == GlobalVariable.GAMEMODE.FACEMODE)
        {
            string result = AnimScore.Instance.AnalyzePlayerEmotionProfile();
            textTitle.text = result;
        }
        else if (GlobalVariable.gamemode == GlobalVariable.GAMEMODE.SMARTWACTH)
        {
            string result = GetEmotionResult(heartRateData);
            textTitle.text = result;
        }
        else
        {
            Debug.Log("Data set Mode default");
        }
        SetAnim();
    }
    public string GetEmotionResult(List<int> heartRateData)
    {
        if (heartRateData == null || heartRateData.Count == 0)
            return "No data recorded.";

        // Calculate stats
        float avg = 0f;
        int min = int.MaxValue;
        int max = int.MinValue;

        for (int i = 0; i < heartRateData.Count; i++)
        {
            int hr = heartRateData[i];
            avg += hr;
            if (hr < min) min = hr;
            if (hr > max) max = hr;
        }

        avg /= heartRateData.Count;
        float variance = max - min; // Heart rate variability

        // Determine emotion profile
        string coreEmotion = "";
        string reaction = "";
        string control = "";
        int focus = Mathf.Clamp(Mathf.RoundToInt(100 - variance), 0, 100);




        // Example logic based on average HR and variance
        if (avg < 72)
        {
            coreEmotion = "Calm Dominator";
            iconTiitle.sprite = iconEmot[5];
        }

        else if (avg < 85)
        {
            coreEmotion = "Strategic Challenger";
            iconTiitle.sprite = iconEmot[4];
        }

        else
        {
            coreEmotion = "Emotional Fighter";
            iconTiitle.sprite = iconEmot[3];
        }


        if (variance > 25)
        {
            reaction = "You lose control when provoked.";
            iconTiitle.sprite = iconEmot[0];
        }

        else if (variance > 15)
        {
            reaction = "You react fast under stress.";
            iconTiitle.sprite = iconEmot[1];
        }

        else
        {
            reaction = "You maintain perfect composure.";
            iconTiitle.sprite = iconEmot[2];
        }


        // Bonus analysis for control
        control = avg > 95 ? "🔥 Needs emotional discipline."
                 : avg > 85 ? "⚡ Controlled but reactive."
                 : "😌 Excellent control.";

        return $"{coreEmotion}\n" +
               $"{reaction}\n" +
               $"Your emotional focus is {focus}%\n" +
               $"{control}";

    }

    void SetAnim()
    {
        Debug.Log("SetAnim");
        icontiitleAnim.DOAnchorPosX(0, 1f).SetEase(Ease.OutBack);
        tiitleTextAnim.DOAnchorPosX(0, 1f).SetEase(Ease.OutBack);
    }
}
