using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardManager : MonoBehaviour
{
    public static CardManager instance;

    public int round;
    public Transform parentCard;
    private Vector2[] originalPositions;
    public string[] categoryCard;
    public TextMeshProUGUI[] categoryText;
    public CanvasGroup CartOptionCatagory;
    public SimpleAgoraController_Unified controller;
    [SerializeField] private List<CanvasGroup> canvasGroups;

    [SerializeField] private TextMeshProUGUI infoTextCategory, valueScore;
    public GameObject waiting;
    private TextMeshProUGUI infoTextwaiting;

    public CanvasGroup infobeforeSelectCard;
    public TextMeshProUGUI infoTextbeforeSelectCard;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        round = 1;
        infoTextwaiting = waiting.GetComponentInChildren<TextMeshProUGUI>();
        originalPositions = new Vector2[parentCard.childCount];
        for (int i = 0; i < parentCard.childCount; i++)
        {
            originalPositions[i] = parentCard.GetChild(i).position;
        }

        for (int i = 0; i < parentCard.childCount; i++)
        {
            int s = i;
            Transform card = parentCard.GetChild(s);
            CanvasGroup cg = card.GetChild(0).GetComponent<CanvasGroup>();
            canvasGroups.Add(cg);
            Vector2 targetPosition = originalPositions[s];
            card.position = targetPosition * 10f;
        }

        controller.OnChatMessage += (uid, text) =>
        {
            SetCategory($"{text}");
        };
    }

    // ============================ FIXED AREA ============================
    public void SHowWaiting2()
    {
        round = 2;
        StartCoroutine(WaitAndShowWaiting());
    }

    private IEnumerator WaitAndShowWaiting()
    {
        // Tunggu sampai nickNameAgoraRemote punya value (maks 5 detik)
        float timeout = 5f;
        while (string.IsNullOrEmpty(GlobalVariable.nickNameAgoraRemote) && timeout > 0)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        // Fade in UI
        CanvasGroup cg = waiting.GetComponent<CanvasGroup>();
        cg.alpha = 0;
        cg.DOFade(1, 0.5f);

        // Jalankan efek ketik teks
        string fullText = $"Round 1 is over, {GlobalVariable.nickNameAgoraRemote} turn to choose a category and ask you a question.";
        infoTextwaiting.text = "";
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(TypeWriterEffect(fullText, infoTextwaiting, 0.03f));
    }
    // ====================================================================

    public void StartGameRound2()
    {
        round = 2;
        if (!controller.isMaster)
        {
            StartCoroutine(PolaGame());
        }
    }

    public void ShowCard()
    {
        if (controller.isMaster)
        {
            StartCoroutine(PolaGame());
        }
    }

    IEnumerator PolaGame()
    {
        infobeforeSelectCard.DOFade(1, 0.5f);
        infoTextbeforeSelectCard.text = "";
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(TypeWriterEffect("You are now a Host", infoTextbeforeSelectCard, 0.05f));

        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(TypeWriterEffect($"Please select a card and ask {GlobalVariable.nickNameAgoraRemote} according to the selected category", infoTextbeforeSelectCard, 0.05f));

        yield return new WaitForSeconds(2f);
        SetCardAnimation();
    }

    IEnumerator TypeWriterEffect(string fullText, TextMeshProUGUI targetText, float delay)
    {
        targetText.text = "";
        foreach (char c in fullText)
        {
            targetText.text += c;
            yield return new WaitForSeconds(delay);
        }
    }

    public void SetCardAnimation()
    {
        CartOptionCatagory.DOFade(1, 0.2f);
        CartOptionCatagory.interactable = true;
        CartOptionCatagory.blocksRaycasts = true;
        StartCoroutine(RandomText());
        for (int i = 0; i < parentCard.childCount; i++)
        {
            Transform card = parentCard.GetChild(i);
            Vector2 targetPosition = originalPositions[i];
            card.DOMove(targetPosition, 1f).SetEase(Ease.OutBack);
            card.DORotateQuaternion(Quaternion.Euler(0, 180, 0), 0.7f)
                .SetEase(Ease.OutBack)
                .SetLoops(5)
                .OnComplete(() => { card.DORotateQuaternion(Quaternion.Euler(0, 0, 0), 0.3f); });
            infobeforeSelectCard.alpha = 0;
            infobeforeSelectCard.interactable = false;
            infobeforeSelectCard.blocksRaycasts = false;
        }
    }

    void SendCatergory(string cat)
    {
        Debug.Log("SendCatergory: " + cat);
        controller.SendChatText("catagorykjdufjk" + cat);
        infoTextCategory.text = cat;
        StartCoroutine(ResetCard());
    }

    IEnumerator RandomText()
    {
        int idx = 0;
        string[] str = new string[categoryText.Length];

        while (idx < 5)
        {
            if (idx % 2 == 0)
            {
                foreach (var cg in canvasGroups)
                {
                    cg.DOFade(0, 0.35f);
                }
            }
            else
            {
                foreach (var cg in canvasGroups)
                {
                    cg.DOFade(1, 0.35f);
                }
            }

            for (int i = 0; i < categoryText.Length; i++)
            {
                int randomIndex = Random.Range(0, categoryCard.Length);
                categoryText[i].text = categoryCard[randomIndex];
                str[i] = categoryCard[randomIndex];
            }

            yield return new WaitForSeconds(0.7f);
            idx++;
        }

        for (int i = 0; i < categoryText.Length; i++)
        {
            int s = i;
            Button btn = categoryText[s].transform.parent.parent.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                SendCatergory(str[s]);
                canvasGroups[s].DOFade(1, 0.5f);
            });
        }
    }

    public void MoveCard(Transform card, Transform target, float duration = 0.5f)
    {
        card.DOMove(target.position, duration);
        card.DORotateQuaternion(target.rotation, duration);
    }

    void SetCategory(string txt)
    {
        if (!txt.Contains("catagorykjdufjk"))
            return;

        string rslt1 = txt.Replace("catagorykjdufjk", "");
        string rslt = rslt1.Replace(GlobalVariable.nickNameAgoraRemote, "");
        infoTextCategory.text = rslt;
        infoTextwaiting.text = GlobalVariable.nickNameAgoraRemote + " chooses a category " + rslt;
        StartCoroutine(HideWaitingInfo(rslt));
    }

    IEnumerator HideWaitingInfo(string rslt)
    {
        infoTextwaiting.text = GlobalVariable.nickNameAgoraRemote + " chooses a category " + rslt;
        yield return new WaitForSeconds(3f);
        infoTextwaiting.text = "are you ready";
        yield return new WaitForSeconds(1f);
        infoTextwaiting.text = "GO";
        yield return new WaitForSeconds(1f);
        waiting.GetComponent<CanvasGroup>().DOFade(0, 0.5f);
    }

    IEnumerator ResetCard()
    {
        yield return new WaitForSeconds(3f);
        for (int i = 0; i < parentCard.childCount; i++)
        {
            int s = i;
            Transform card = parentCard.GetChild(s);
            Vector2 targetPosition = originalPositions[s];
            card.DOMove(targetPosition * 10f, 5f).SetEase(Ease.OutBack);
            canvasGroups[s].alpha = 0;
        }
        yield return new WaitForSeconds(1f);
        CartOptionCatagory.DOFade(0, 0.5f);
        GameScoreManager.instance.StartCountdown();
    }
}
