using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UserList : MonoBehaviour
{
    public TextMeshProUGUI nickNameUser;
    public TextMeshProUGUI statusOnline;
    public Image avatar;
    public Button btnAddFriend;
    public Image imgStatusOnline;

    public Color onlineColor;
    public Color offlineColor;


    public void SetUserlist(Sprite spAvatar, string _nickName, bool isOnline)
    {
        avatar.sprite = spAvatar;
        nickNameUser.text = _nickName;
        if (isOnline)
        {
            statusOnline.text = "ONLINE";
            statusOnline.color = onlineColor;
            imgStatusOnline.color = onlineColor;
            return;
        }
        statusOnline.text = "OFFLINE";
        statusOnline.color = offlineColor;
        imgStatusOnline.color = offlineColor;
    }
}
