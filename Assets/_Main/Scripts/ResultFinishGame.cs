using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultFinishGame : MonoBehaviour
{
    public SimpleAgoraController_Unified controller;
    public static ResultFinishGame Instance;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI statusText;
    public Button buttonOK;
    public Image avatar;
    public Sprite defaultAvatar;
    public TextMeshProUGUI nickNameText;
    public TextMeshProUGUI totalScoreText;
    [SerializeField] private CanvasGroup cvsPanelResult;

    [SerializeField] private AnimResult animResult;
    public CanvasGroup canvasMain;
    public CanvasGroup finishPanel;

    private void Awake()
    {
        Instance = this;

        cvsPanelResult.alpha = 0;
        cvsPanelResult.blocksRaycasts = false;
        cvsPanelResult.interactable = false;
    }

    public void ShowResult(string score, string totalScore, bool isWin)
    {
        canvasMain.alpha = 0;
        animResult.PlayAnimation();
        SetProfile(GlobalVariable.avatarLocal, GlobalVariable.nickNameAgora);
        cvsPanelResult.alpha = 0;
        cvsPanelResult.DOFade(1, 0.25f);
        cvsPanelResult.blocksRaycasts = true;
        cvsPanelResult.interactable = true;
        scoreText.text = score;
        totalScoreText.text = totalScore;

        statusText.text = isWin ? "YOU WIN!" : "YOU LOSE!";
        statusText.color = isWin ? Color.green : Color.red;

        buttonOK.onClick.RemoveAllListeners();
        buttonOK.onClick.AddListener(() =>
        {
            // MainMenuSnapBattle.Instance.OnRestart();
            FinishGame();
        });
    }

    public void HideResult()
    {
        cvsPanelResult.DOFade(0, 0.25f).OnComplete(() =>
        {
            cvsPanelResult.blocksRaycasts = false;
            cvsPanelResult.interactable = false;
        });
    }

    public void SetProfile(Sprite avata, string nickName)
    {
        nickNameText.text = nickName;
        if (avata != null)
        {
            avatar.sprite = avata;
        }
        else
        {
            avatar.sprite = defaultAvatar;
        }
    }

    public void FinishGame()
    {
        buttonOK.interactable = false;
        Loading.instance.ShowLoading();
        finishPanel.DOFade(1, 0.2f);
        if (GlobalVariable.STATUS != Status.PLAYING)
        {
            cvsPanelResult.DOFade(1, 0.25f).OnComplete(() =>
            {
                cvsPanelResult.blocksRaycasts = false;
                cvsPanelResult.interactable = false;
                controller.LeaveChannel(true);
            });
        }
        else
        {
            cvsPanelResult.DOFade(1, 0.25f).OnComplete(() =>
            {
                cvsPanelResult.blocksRaycasts = false;
                cvsPanelResult.interactable = false;
                controller.LeaveChannel(false);
            });
        }

    }


}
