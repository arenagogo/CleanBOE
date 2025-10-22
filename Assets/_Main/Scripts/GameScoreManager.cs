using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;

public class GameScoreManager : MonoBehaviour
{
    public RectTransform posScoreLocal, posScoreRemote, posScore;
    public GameObject textScoreAnim;
    private Vector3 posScoreCure;
    public HeartRateResultAnalyzer heartRateResultAnalyzer;
    public List<int> heartRateData = new List<int>();
    public static GameScoreManager instance;
    public SimpleAgoraController_Unified controller;
    public TextMeshProUGUI timeCountdown;
    public float timeGame = 60f;

    public CardManager cardManager;
    public bool gameEnded = false;

    [SerializeField] private float scoreLocal = 0;
    [SerializeField] private float scoreRemote = 0;

    public TextMeshProUGUI statusPlayerText;
    public TextMeshProUGUI scorePlayerLocalText;
    public TextMeshProUGUI scorePlayerRemoteText;
    public CanvasGroup panelEndGame;
    public RtmChannelManager rtmChannelManager;

    public float bpmLocal;
    public float bpmRemote;

    public bool gameOnPlaying = false;

    public TextMeshProUGUI scoreProfile;
    public TextMeshProUGUI scoreFinish;

    public GameObject ButtonScanFace;

    private void OnEnable()
    {
        // Subscribe ke event
        SimpleAgoraController_Unified.OnScoreChanged += UpdateScoreUI;
    }

    private void OnDisable()
    {
        // Unsubscribe biar aman
        SimpleAgoraController_Unified.OnScoreChanged -= UpdateScoreUI;
    }

    private void UpdateScoreUI(int newScore)
    {
        scoreProfile.text = newScore.ToString("F0");
        scoreFinish.text = newScore.ToString("F0");
    }

    private void Awake()
    {
        instance = this;
        scorePlayerLocalText.text = "";
        scorePlayerRemoteText.text = "";
    }

    private void Start()
    {
        posScoreCure = posScore.anchoredPosition;
        controller.OnChatMessage += (uid, text) =>
        {
            StartCountdownRemote($"{text}");
        };
    }

    public void UpdateBPMLocal(float bpm)
    {
        bpmLocal = bpm;
    }

    public void UpdateBPMRemote(float bpm)
    {
        bpmRemote = bpm;
    }

    public void StartCountdown()
    {
        _CountdownCoroutine = StartCoroutine(CountdownCoroutine());

        controller.SendChatText("StartGame_kjsfjj6dyhah");
        statusPlayerText.text = "Ask Questions To " + GlobalVariable.nickNameAgoraRemote;
    }

    public void StartCountdownRemote(string starting)
    {
        if (!starting.Contains("StartGame_kjsfjj6dyhah"))
            return;

        _CountdownCoroutine = StartCoroutine(CountdownCoroutine());
        statusPlayerText.text = "Answer Questions From " + GlobalVariable.nickNameAgoraRemote;
    }


    public void UpdateScoreRemote(string userNameSender, string _score, string battleID)
    {
        Debug.Log($"papahjahasekali {userNameSender}  {_score} {battleID} {GlobalVariable.nickNameAgoraRemote} {GlobalVariable.nickNameAgora}");
        if (userNameSender != GlobalVariable.nickNameAgoraRemote)
            return;

        if (battleID != controller.battleId)
            return;


        string cleanScore = CleanText(_score);
        string digits = ExtractDigits(cleanScore);
        if (int.TryParse(digits, out int parsedScore))
        {
            scoreRemote = parsedScore;
            scorePlayerRemoteText.text = scoreRemote.ToString("F0");
        }
        else
        {
            Debug.LogWarning("Failed to parse score from string: " + _score);
        }
    }


    public void EndCountdownRemote(string starting)
    {
        if (!starting.Contains("Ended_kjsfjj6dyhah"))
            return;

        GameEnded();
    }

    Coroutine _CountdownCoroutine;

    public void SetScoreFaceMode(int _scoreLocal, int _scoreRemote)
    {
        if (_scoreLocal > 0)
            scoreLocal += _scoreLocal;
        if (_scoreRemote > 0)
            scoreRemote += _scoreRemote;

        scorePlayerLocalText.text = scoreLocal.ToString();
        scorePlayerRemoteText.text = scoreRemote.ToString();
    }

    private IEnumerator CountdownCoroutine()
    {
        float remainingTime = timeGame;
        gameOnPlaying = true;   // ✅ aktif selama countdown

        if (GlobalVariable.gamemode == GlobalVariable.GAMEMODE.FACEMODE)
        {
            if (controller.isMaster && cardManager.round == 1)
                ButtonScanFace.SetActive(true);
            else if (!controller.isMaster && cardManager.round == 2)
                ButtonScanFace.SetActive(true);
            else
                ButtonScanFace.SetActive(false);
        }

        if (GlobalVariable.gamemode == GlobalVariable.GAMEMODE.SMARTWACTH)
        {
            while (remainingTime > 0)
            {
                timeCountdown.text = Mathf.Ceil(remainingTime).ToString();

                // Hitung skor sekali per detik
                float diff = Mathf.Abs(bpmLocal - bpmRemote);
                if (bpmLocal < bpmRemote)
                {
                    scoreLocal += diff;   // ✅ local dapat poin
                }

                scorePlayerLocalText.text = scoreLocal.ToString("F0");

                rtmChannelManager.SendScore(scoreLocal.ToString("F0"), GlobalVariable.nickNameAgora, controller.battleId);

                if (diff > 0)
                {
                    if (bpmLocal < bpmRemote)
                    {
                        GameObject score = Instantiate(textScoreAnim, posScore);
                        score.GetComponent<TextMeshProUGUI>().text = diff.ToString();
                        score.transform.DOMove(posScoreLocal.position, 0.5f).SetEase(Ease.InBounce).OnComplete(() => Destroy(score));
                    }
                    else
                    {
                        GameObject score = Instantiate(textScoreAnim, posScore);
                        score.GetComponent<TextMeshProUGUI>().text = diff.ToString();
                        score.transform.DOMove(posScoreRemote.position, 0.5f).SetEase(Ease.InBounce).OnComplete(() => Destroy(score));
                    }
                }






                yield return new WaitForSeconds(1f);
                remainingTime -= 1f;
            }

        }
        else
        {
            while (remainingTime > 0)
            {
                timeCountdown.text = Mathf.Ceil(remainingTime).ToString();
                yield return new WaitForSeconds(1f);
                remainingTime -= 1f;
            }
        }



        gameOnPlaying = false;  // ❌ selesai

        if (GlobalVariable.gamemode == GlobalVariable.GAMEMODE.FACEMODE)
        {
            ButtonScanFace.SetActive(false);
        }

        timeCountdown.text = "0";

        if (!gameEnded)
        {
            CardManager.instance.StartGameRound2();
            if (controller.isMaster)
                CardManager.instance.SHowWaiting2();
        }
        else
        {
            controller.SendChatText("Ended_kjsfjj6dyhah");
            GameEnded();
        }

        gameEnded = true;
    }

    public void GameEnded()
    {
        Debug.Log("Game Ended");

        GlobalVariable.STATUS = Status.STANDBY;

        if (_CountdownCoroutine != null)
        {
            StopCoroutine(_CountdownCoroutine);
            _CountdownCoroutine = null;
        }

        panelEndGame.DOFade(1, 0.5f);

        if (scoreLocal > scoreRemote)
        {

            controller.Addscore((int)scoreLocal, "WIN");
            Debug.Log("YOU WIN");
            ResultFinishGame.Instance.ShowResult(((int)scoreLocal).ToString(), "loremIpsum", true);
        }

        else
        {
            controller.Addscore((int)scoreLocal, "LOSE");
            Debug.Log("YOU LOSE");
            ResultFinishGame.Instance.ShowResult(((int)scoreLocal).ToString(), "loremIpsum", false);
        }

        heartRateResultAnalyzer.SetTittle(heartRateData);
    }

    public void GameEndedRemoteLeave()
    {

        if (_CountdownCoroutine != null)
        {
            StopCoroutine(_CountdownCoroutine);
            _CountdownCoroutine = null;
        }

        panelEndGame.DOFade(1, 0.5f);

        controller.Addscore((int)scoreLocal, "WIN");
        Debug.Log("YOU WIN");

        ResultFinishGame.Instance.ShowResult(((int)scoreLocal).ToString(), GlobalVariable.nickNameAgoraRemote, true);
        heartRateResultAnalyzer.SetTittle(heartRateData);
    }

    string CleanText(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";
        // Hilangkan semua karakter non-digit & non-huruf
        return new string(input.Where(c => char.IsLetterOrDigit(c)).ToArray());
    }

    string ExtractDigits(string input)
    {
        if (string.IsNullOrEmpty(input)) return "0";
        return Regex.Match(input, @"\d+").Value;
    }
}
