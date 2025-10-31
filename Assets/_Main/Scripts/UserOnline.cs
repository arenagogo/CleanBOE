using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UserOnline : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI nickName;
    public TextMeshProUGUI modeText;
    public TextMeshProUGUI buttonText;
    public TextMeshProUGUI readyToPlayStatus;
    public Image ReadyStatusIcon;

    public Button actionButton;

    public Image buttonImage;

    public bool isOnline = true;

    public string uid;

    [SerializeField] private float timerOffline = 30f;

    public void IsonlineStatus()
    {
        isOnline = true;
        timerOffline = 30f;
    }

    void Update()
    {
        if (timerOffline <= 0f)
        {
            isOnline = false;
            readyToPlayStatus.text = "Offline";
            readyToPlayStatus.color = Color.black;
            actionButton.interactable = false;
            readyToPlayStatus.color = Color.black;
            ReadyStatusIcon.color = Color.black;
            buttonText.text = "OFFLINE";
        }
        else
        {
            timerOffline -= Time.deltaTime;
        }
    }

}
