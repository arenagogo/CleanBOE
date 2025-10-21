using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnimScore : MonoBehaviour
{
    public static AnimScore Instance;
    public Slider angryTextScore;
    public Slider disgustTextScore;
    public Slider fearTextScore;
    public Slider happyTextScore;
    public Slider sadTextScore;
    public Slider surpriseTextScore;
    public Slider neutralTextScore;
    public Image iconResult;
    public TextMeshProUGUI textresult;

    public CanvasGroup faceNoteDetectedcvs, faceDetectedcvs;

    public Button hitButton;
    private CanvasGroup cvsHitButton;


    public TextMeshProUGUI totalScore;

    public List<ScoreData> scoreDataListRemote = new List<ScoreData>();
    public List<ScoreData> scoreDataListLocal = new List<ScoreData>();
    public Sprite[] emotionIcon;

    public GameScoreManager gameScoreManager;
    public int curScoreFaceMode = 0;

    void Awake()
    {
        Instance = this;
        transform.localScale = Vector3.zero;
        cvsHitButton = hitButton.GetComponent<CanvasGroup>();
        // scanerAnim = hitButton.GetComponent<ScanerAnim>();
    }

    void Start()
    {
        angryTextScore.maxValue = 100;
        disgustTextScore.maxValue = 100;
        fearTextScore.maxValue = 100;
        happyTextScore.maxValue = 100;
        sadTextScore.maxValue = 100;
        surpriseTextScore.maxValue = 100;
        neutralTextScore.maxValue = 100;
    }

    private void ShowScoreX(float angryScore, float disgustScore, float fearScore, float happyScore, float sadScore, float surpriseScore, float neutralScore, bool faceDetected, string statusOwner, string owner)
    {
        if (faceDetected)
        {
            faceDetectedcvs.alpha = 1;
            faceDetectedcvs.interactable = true;
            faceDetectedcvs.blocksRaycasts = true;

            faceNoteDetectedcvs.alpha = 0;
            faceNoteDetectedcvs.interactable = false;
            faceNoteDetectedcvs.blocksRaycasts = false;
        }
        else
        {
            faceDetectedcvs.alpha = 0;
            faceDetectedcvs.interactable = false;
            faceDetectedcvs.blocksRaycasts = false;

            faceNoteDetectedcvs.alpha = 1;
            faceNoteDetectedcvs.interactable = true;
            faceNoteDetectedcvs.blocksRaycasts = true;

            transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.InBack);
            transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack).SetDelay(2.0f).OnComplete(() =>
            {
                hitButton.interactable = true;
            });
        }


        // Clear sliders (set to 0) before animating
        if (angryTextScore != null) angryTextScore.value = 0f;
        if (disgustTextScore != null) disgustTextScore.value = 0f;
        if (fearTextScore != null) fearTextScore.value = 0f;
        if (happyTextScore != null) happyTextScore.value = 0f;
        if (sadTextScore != null) sadTextScore.value = 0f;
        if (surpriseTextScore != null) surpriseTextScore.value = 0f;
        if (neutralTextScore != null) neutralTextScore.value = 0f;
        totalScore.text = "";
        transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            // Animate slider scales and values with staggered delays to mimic previous timing
            if (angryTextScore != null)
            {
                angryTextScore.DOValue(angryScore, 0.5f).SetEase(Ease.OutBack).SetDelay(0f);
            }
            if (disgustTextScore != null)
            {
                disgustTextScore.DOValue(disgustScore, 0.5f).SetEase(Ease.OutBack).SetDelay(0.1f);
            }
            if (fearTextScore != null)
            {
                fearTextScore.DOValue(fearScore, 0.5f).SetEase(Ease.OutBack).SetDelay(0.2f);
            }
            if (happyTextScore != null)
            {
                happyTextScore.DOValue(happyScore, 0.5f).SetEase(Ease.OutBack).SetDelay(0.3f);
            }
            if (sadTextScore != null)
            {
                sadTextScore.DOValue(sadScore, 0.5f).SetEase(Ease.OutBack).SetDelay(0.4f);
            }
            if (surpriseTextScore != null)
            {
                surpriseTextScore.DOValue(surpriseScore, 0.5f).SetEase(Ease.OutBack).SetDelay(0.5f);
            }
            if (neutralTextScore != null)
            {
                neutralTextScore.DOValue(neutralScore, 0.5f).SetEase(Ease.OutBack).SetDelay(0.6f);
            }

            // Determine highest emotion and update iconResult and textresult
            // Order of emotionIcon: 0=angry,1=disgust,2=fear,3=happy,4=sad,5=surprise,6=neutral
            float[] values = new float[] { angryScore, disgustScore, fearScore, happyScore, sadScore, surpriseScore, neutralScore };
            int maxIndex = 0;
            float maxValue = values[0];
            for (int i = 1; i < values.Length; i++)
            {
                if (values[i] > maxValue)
                {
                    maxValue = values[i];
                    maxIndex = i;
                }
            }

            // Set icon if available
            if (iconResult != null && emotionIcon != null && emotionIcon.Length > maxIndex)
            {
                iconResult.sprite = emotionIcon[maxIndex];
                iconResult.SetNativeSize();
            }

            // Set textresult to emotion name and percentage (e.g., "Angry 72%")
            string[] emotionNames = new string[] { "Angry", "Disgust", "Fear", "Happy", "Sad", "Surprise", "Neutral" };
            if (textresult != null)
            {
                int percent = (int)(maxValue * 100f);
                textresult.text = emotionNames[maxIndex] + " " + percent.ToString() + "%";
            }

            float total1 = angryScore + disgustScore + fearScore + sadScore + surpriseScore;
            float total2 = happyScore + neutralScore;
            float _total = total1 - total2;
            int total = (int)_total;

            // int total = (int)(angryScore + disgustScore + fearScore + sadScore + surpriseScore - (happyScore + neutralScore));


            int startValue = 0;
            DOTween.To(() => startValue, x =>
            {
                startValue = x;
                totalScore.text = startValue.ToString();
            },
            total, // target akhir
            1.0f   // durasi animasi (1 detik, bisa diubah)
            ).SetEase(Ease.OutQuad).SetDelay(0.6f).OnComplete(() =>
            {
                transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack).SetDelay(1.0f).OnComplete(() =>
                {
                    cvsHitButton.alpha = 1;
                    cvsHitButton.interactable = true;
                    iconResult.sprite = null;
                    // scanerAnim.StartLoop();
                });
            });
        });

        ScoreData newScoreData = new ScoreData(angryScore, disgustScore, fearScore, happyScore, sadScore, surpriseScore, neutralScore, faceDetected, owner);
        if (statusOwner == "remote")
        {
            scoreDataListRemote.Add(newScoreData);
        }

        else
        {
            scoreDataListLocal.Add(newScoreData);
        }

        if (owner == GlobalVariable.nickNameAgora)
            gameScoreManager.SetScoreFaceMode(0, newScoreData.total);
        else
            gameScoreManager.SetScoreFaceMode(newScoreData.total, 0);
    }

    public void ReceiverDataScore(string jsonData)
    {
        ScoreData receivedData = JsonUtility.FromJson<ScoreData>(jsonData);
        float angryScore = receivedData.angry;
        float disgustScore = receivedData.disgust;
        float fearScore = receivedData.fear;
        float happyScore = receivedData.happy;
        float sadScore = receivedData.sad;
        float surpriseScore = receivedData.surprise;
        float neutralScore = receivedData.neutral;
        bool faceDetected = receivedData.faceDetected;
        ShowScoreX(angryScore, disgustScore, fearScore, happyScore, sadScore, surpriseScore, neutralScore, faceDetected, "remote", GlobalVariable.nickNameAgoraRemote);
    }

    public void SendDataToRTM(float angryScore, float disgustScore, float fearScore, float happyScore, float sadScore, float surpriseScore, float neutralScore, bool faceDetected)
    {
        ScoreData scoreData = new ScoreData(angryScore, disgustScore, fearScore, happyScore, sadScore, surpriseScore, neutralScore, faceDetected, GlobalVariable.nickNameAgora);
        string jsonData = JsonUtility.ToJson(scoreData);
        RtmChannelManager.instant.GoSendTrackFace(jsonData);
        ShowScoreX(angryScore, disgustScore, fearScore, happyScore, sadScore, surpriseScore, neutralScore, faceDetected, "local", GlobalVariable.nickNameAgora);
    }

    [Serializable]
    public class ScoreData
    {
        public float angry;
        public float disgust;
        public float fear;
        public float happy;
        public float sad;
        public float surprise;
        public float neutral;

        public int total;

        public bool faceDetected;

        public string owner;


        public ScoreData(float angry, float disgust, float fear, float happy, float sad, float surprise, float neutral, bool faceDetected, string _owner)
        {
            this.angry = angry;
            this.disgust = disgust;
            this.fear = fear;
            this.happy = happy;
            this.sad = sad;
            this.surprise = surprise;
            this.neutral = neutral;
            this.faceDetected = faceDetected;
            this.owner = _owner;
            total = (int)((angry + disgust + fear + sad + surprise) - (happy + neutral));
        }
    }

    public string AnalyzePlayerEmotionProfile()
    {
        if (scoreDataListLocal == null || scoreDataListLocal.Count == 0)
            return "No data yet. Try playing to analyze your emotion profile.";

        // Rata-rata setiap emosi
        float avgAngry = 0, avgDisgust = 0, avgFear = 0, avgHappy = 0, avgSad = 0, avgSurprise = 0, avgNeutral = 0;

        foreach (var s in scoreDataListLocal)
        {
            avgAngry += s.angry;
            avgDisgust += s.disgust;
            avgFear += s.fear;
            avgHappy += s.happy;
            avgSad += s.sad;
            avgSurprise += s.surprise;
            avgNeutral += s.neutral;
        }

        int count = scoreDataListLocal.Count;
        avgAngry /= count;
        avgDisgust /= count;
        avgFear /= count;
        avgHappy /= count;
        avgSad /= count;
        avgSurprise /= count;
        avgNeutral /= count;

        // Identifikasi emosi dominan
        float[] values = { avgAngry, avgDisgust, avgFear, avgHappy, avgSad, avgSurprise, avgNeutral };
        string[] names = { "Angry", "Disgust", "Fear", "Happy", "Sad", "Surprise", "Neutral" };
        int dominantIndex = 0;
        for (int i = 1; i < values.Length; i++)
        {
            if (values[i] > values[dominantIndex])
                dominantIndex = i;
        }

        string dominantEmotion = names[dominantIndex];
        float focusScore = Mathf.Clamp01(1f - Mathf.Abs(avgNeutral - 0.5f)) * 100f; // seberapa stabil netralnya

        // Tentukan "title" berdasarkan pola
        string title = "";
        if (dominantEmotion == "Neutral" && avgHappy > 0.4f)
            title = "Your core emotion: Calm Dominator";
        else if (avgFear < 0.3f && avgAngry < 0.4f && avgHappy > 0.5f)
            title = "You react fast under stress";
        else if (focusScore > 70)
            title = $"Your emotional focus is {focusScore:F0}%";
        else if (avgAngry > 0.5f || avgDisgust > 0.5f)
            title = "You lose control when provoked";
        else if (avgSad > 0.4f && avgHappy < 0.3f)
            title = "You carry emotions deeply";
        else
            title = "Balanced emotion player";

        // Tambahan info
        string summary =
           $"Dominant Emotion: <color=#FF0000>{dominantEmotion}</color>\n" +
           $"Avg Focus: <color=#0099FF>{focusScore:F1}%</color>\n" +
           $"Profile: <color=#00FF00>{title}</color>";

        Debug.Log("[Emotion Analysis]\n" + summary);
        return summary;
    }


}
