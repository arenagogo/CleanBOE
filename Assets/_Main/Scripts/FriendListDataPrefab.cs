using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FriendListDataPrefab : MonoBehaviour
{
    public TextMeshProUGUI nickNameFriend;
    public Image avatar;
    public TextMeshProUGUI onlineStatus;
    public Image imgStatusOnline;

    public Color onlineColor;
    public Color offlineColor;

    public Button btnInvite;
    public string nickNameFriendForInvite;

    public void SetFriendlist(Sprite spAvatar, string _nickName)
    {
        avatar.sprite = spAvatar;
        nickNameFriend.text = _nickName;
        nickNameFriendForInvite = _nickName;
        onlineStatus.text = "OFFLINE";
        onlineStatus.color = offlineColor;
        imgStatusOnline.color = offlineColor;
        btnInvite.gameObject.SetActive(false);


        //if (_onlineStatus)
        //{
        //    onlineStatus.text = "ONLINE";
        //    onlineStatus.color = onlineColor;
        //    imgStatusOnline.color = onlineColor; btnInvite.gameObject.SetActive(true);
        //}
        //else
        //{
        //    onlineStatus.text = "OFFLINE";
        //    onlineStatus.color = offlineColor;
        //    imgStatusOnline.color = offlineColor;
        //    btnInvite.gameObject.SetActive(false);
        //}
    }

    public void SetFriendlist_ONLINE()
    {
            onlineStatus.text = "ONLINE";
            onlineStatus.color = onlineColor;
            imgStatusOnline.color = onlineColor; btnInvite.gameObject.SetActive(true);
    }

    public void SetFriendlist_OFFLINE()
    {
        onlineStatus.text = "OFFLINE";
        onlineStatus.color = offlineColor;
        imgStatusOnline.color = offlineColor;
        btnInvite.gameObject.SetActive(false);
    }


}
