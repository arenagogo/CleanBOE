using TMPro;

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class HistoryDetail : MonoBehaviour
{
    public Image avatarRemote;
    public TextMeshProUGUI nickNameRemote;
    public TextMeshProUGUI date;
    public TextMeshProUGUI statusLocal, statusRemote, nickNameRemoteOnStatus, getScore;
    public GameObject greenLocal, redLocal;
    public GameObject greenRemote, redRemote;
    private CanvasGroup cvs;

    private void Awake()
    {
        cvs = GetComponent<CanvasGroup>();
        ResetDetailUI();
    }

    /// <summary>
    /// Set detail data untuk history battle
    /// </summary>
    public void SetDetail(string _nickNameRemote, string battleDate, string remoteNameOnStatus, string score, string status)
    {
        if (nickNameRemote != null) nickNameRemote.text = _nickNameRemote;
        if (date != null) date.text = battleDate;

       // if (statusLocal != null) statusLocal.text = localStatus;
       // if (statusRemote != null) statusRemote.text = remoteStatus;

        if (nickNameRemoteOnStatus != null) nickNameRemoteOnStatus.text = remoteNameOnStatus;
        if (getScore != null) getScore.text = score;

        if(status == "WIN")
        {
            greenLocal.SetActive(true);
            redLocal.SetActive(false);
            greenRemote.SetActive(false);
            redRemote.SetActive(true);
            statusLocal.text = "WIN";
            statusRemote.text = "LOSE";
        }
        else
        {
            greenLocal.SetActive(false);
            redLocal.SetActive(true);
            greenRemote.SetActive(true);
            redRemote.SetActive(false);
            statusLocal.text = "LOSE";
            statusRemote.text = "WIN";
        }
    }

    public IEnumerator SetDetail(Sprite avatar)
    {
        if (avatarRemote != null) 
            avatarRemote.sprite = avatar;

        yield return new WaitUntil(() => avatarRemote.sprite != null);
        cvs.alpha = 1;
        cvs.interactable = true;
        cvs.blocksRaycasts = true;
    }

    public void ResetDetailUI()
    {
        if (avatarRemote != null) avatarRemote.sprite = null;
        if (nickNameRemote != null) nickNameRemote.text = "";
        if (date != null) date.text = "";

        // if (statusLocal != null) statusLocal.text = localStatus;
        // if (statusRemote != null) statusRemote.text = remoteStatus;

        if (nickNameRemoteOnStatus != null) nickNameRemoteOnStatus.text = "";
        if (getScore != null) getScore.text = "";

        greenLocal.SetActive(false);
        redLocal.SetActive(false);
        greenRemote.SetActive(false);
        redRemote.SetActive(false);
        statusLocal.text = "";
        statusRemote.text = "";

        cvs.alpha = 0;
        cvs.interactable = false;
        cvs.blocksRaycasts = false;
    }
}
