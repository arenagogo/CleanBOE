using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class INVITE : MonoBehaviour
{
    public static INVITE Instance;
    public SimpleAgoraController_Unified controler;
    public Image avatar;
    public TextMeshProUGUI username;
    public TextMeshProUGUI message;
    private CanvasGroup cvs;
    public Sprite defaultAvatar;
    public RtmChannelManager rtmChannelManager;
    public HeartRateEstimator_Unified heartRateEstimator;

    public CanvasGroup[] listHideCvsOnPlaying;

    private void Awake()
    {
        Instance = this;
        cvs = GetComponent<CanvasGroup>();
    }

    public IEnumerator SetInviteOpen(string _username,string _message, string avatarUrl)
    {

        controler.GetSpriteFromURL(avatarUrl, (downloadedSprite) =>
        {
            // Callback ini akan berjalan setelah download selesai
            if (downloadedSprite != null)
            {
                avatar.sprite = downloadedSprite;
            }
            else
            {
                avatar.sprite = defaultAvatar;
                Debug.LogWarning("Gagal mendapatkan sprite dari URL. INVIT");
            }
        });

        yield return new WaitUntil(() => avatar.sprite != null);

        cvs.alpha = 1f;
        cvs.interactable = true;
        cvs.blocksRaycasts = true;
        username.text = _username;
        message.text = _message;
    }

    public void Accept()
    {
        string battleId = GenerateRandomRoomName(8);
        string roomName = GenerateRandomRoomName(8);
        controler.JoinChanelOnInvite(roomName, controler.data.data.profile.username, battleId);
        rtmChannelManager.AcceptInvite(roomName, controler.data.data.profile.username, battleId);
        HideInvite();
        heartRateEstimator.StartVideo();
    }

    public void RejectInvit()
    {
        HideInvite();
        GlobalVariable.STATUS = Status.STANDBY;
        rtmChannelManager.RejectInvite("", "", "");
    }

    string GenerateRandomRoomName(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        System.Random random = new System.Random();
        char[] result = new char[length];

        for (int i = 0; i < length; i++)
        {
            result[i] = chars[random.Next(chars.Length)];
        }

        return new string(result);
    }

    public void HideInvite()
    {
        cvs.alpha = 0f;
        cvs.interactable = false;
        cvs.blocksRaycasts = false;
        avatar.sprite = null;
        username.text = "";
        message.text = "";
    }

    public void JoinChaelInvite()
    {
        foreach (CanvasGroup cvs in listHideCvsOnPlaying)
        {
            cvs.alpha = 0;
            cvs.interactable = false;
            cvs.blocksRaycasts = false;
        }
    }
}
