using DG.Tweening;

using System;
using System.Text.RegularExpressions;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class RtcChatUI : MonoBehaviour
{
    public static RtcChatUI Instance;
    public SimpleAgoraController_Unified controller;
    public TMP_InputField input;
    public Button sendBtn;
    public Transform chatContent;     // parent untuk item text
    public GameObject chatItemPrefab; // prefab: TextMeshProUGUI sederhana
    public AutoScroll autoScroll;
    public TextMeshProUGUI infoText;
    [SerializeField]private int infoCounter = 0;
    public RectTransform panelChat;
    public TextMeshProUGUI remoteBPM;


    CanvasGroup cvsInfo;

    public float hidePos, showPos;

    public Image slidBpm;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        cvsInfo = infoText.GetComponent<CanvasGroup>();
        hidePos = panelChat.anchoredPosition.y;

        sendBtn.onClick.AddListener(() =>
        {
            if (controller.SendChatText(input.text))
            {
                AddBubble($"<color=#0000FF>{GlobalVariable.nickNameAgora}</color>\n<color=#000000>{input.text}</color>");
                input.text = "";
            }
        });

        controller.OnChatMessage += (uid, text) =>
        {
           // Debug.Log($"OnChatMessage: {uid} {text}");
            AddBubble($"{text}");
        };
    }

    void AddBubble(string s)
    {
        if (s.Contains("catagorykjdufjk"))
            return;
        if (s.Contains("StartGame_kjsfjj6dyhah"))
            return;
        if (s.Contains("Ended_kjsfjj6dyhah"))
            return;

        if(s.Contains("mamahjahat"))
        {
            string input = s;
            string keyword = "mamahjahat";
            int index = input.IndexOf(keyword);
            string result = (index >= 0) ? input.Substring(index) : input;

            string jkl = result.Replace("mamahjahat", "").Trim();

            // Buang semua karakter yang bukan angka atau titik
            jkl = Regex.Replace(jkl, @"[^\d\.]", "");

            // Kalau kosong, kasih default
            if (string.IsNullOrEmpty(jkl))
                jkl = "80";

            if (float.TryParse(jkl, out float bpm))
            {
                Debug.Log($"Mendeteksi BPM dalam teks: {jkl} → {bpm}");
                SetBPM(jkl);
                GameScoreManager.instance.UpdateBPMRemote(bpm);
            }
            else
            {
                Debug.LogWarning($"Gagal parse BPM dari string: '{jkl}' (aslinya: '{s}')");
            }



        }
        else if(s.Contains("papahjahat"))
        {
            string text = s;
            Debug.Log("Mendeteksi URL dalam teks: " + s);    
            // cari pola http atau https sampai spasi atau akhir string
            Match m = Regex.Match(text, @"https?:\/\/\S+");

            if (m.Success)
            {
                string url = m.Value;
                Debug.Log("URL ketemu: " + url);
                controller.SetAvatarRemote(url);
            }
            else
            {
                Debug.LogWarning("Tidak ada URL ditemukan");
            }
        }
        else
        {
            var go = Instantiate(chatItemPrefab, chatContent);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            if (tmp) tmp.text = s;

            autoScroll.ScrollToBottom();
            infoCounter++;
            infoText.text = $"{infoCounter}";
            SHoHideInfo();
        }  
    }

    void SetBPM(string bpmx)
    {
        if (float.TryParse(bpmx, System.Globalization.NumberStyles.Float,
                           System.Globalization.CultureInfo.InvariantCulture, out float bpm))
        {
            float targetFill = Mathf.Clamp01(bpm / GlobalVariable.maxHeartRate);

            // pakai DOTween biar smooth
            slidBpm.DOFillAmount(targetFill, 0.5f);

            remoteBPM.text = bpm.ToString("F0"); // tampilkan angka bulat
        }
        else
        {
            Debug.LogWarning($"BPM string tidak valid: {bpmx}");
        }
    }


    //public void HeartRate(string rate)
    //{
    //    controller.SendChatText("");
    //    AddBubble("", false);
    //}

    public void ToggleChat()
    {
        if (panelChat.anchoredPosition.y == hidePos)
        {
            panelChat.DOAnchorPosY(showPos, 0.3f).SetEase(Ease.OutBack);
        }
        else
        {
            panelChat.DOAnchorPosY(hidePos, 0.3f).SetEase(Ease.InBack);
        }

        infoCounter = 0;
        infoText.text = "";
        cvsInfo.alpha = 0;
    }

    void SHoHideInfo()
    {
        if (infoCounter > 0 && panelChat.anchoredPosition.y == hidePos)
        {
            infoText.text = $"{infoCounter}";
            cvsInfo.DOFade(1, 0.3f);
        }
        else
        {
            cvsInfo.DOFade(0, 0.3f);
        }
    }
}
