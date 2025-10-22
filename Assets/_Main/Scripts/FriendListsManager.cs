using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class FriendListsManager : MonoBehaviour
{
    public RtmChannelManager rtmChannelManager;
    public static FriendListsManager instance;
    public SimpleAgoraController_Unified controler;
    public Transform posFrienlist;
    public GameObject frienlist;
    //private string apiMutualFriens = GlobalVariable.baseUrlArenaGO + "/nestjsApi/api/friend-list/mutual-friends";
    public Sprite defaultAvatar;
    public List<GameObject> friendsDataPrefabs;

    public CanvasGroup[] hide;
    public CanvasGroup show;
    public Toggle toggleFriendReq;

    public void OpenFriendRequest()
    {
        if (toggleFriendReq.isOn)
        {
            GetFriendslist();
            show.alpha = 1;
            show.blocksRaycasts = true;
            show.interactable = true;
            foreach (CanvasGroup cvs in hide)
            {
                cvs.alpha = 0;
                cvs.blocksRaycasts = false;
                cvs.interactable = false;
            }
        }

    }



    private void Awake()
    {
        instance = this;
        friendsDataPrefabs.Clear();
        if (posFrienlist.childCount != 0)
        {
            for (int i = 0; i < posFrienlist.childCount; i++)
            {
                Destroy(posFrienlist.GetChild(i).gameObject);
            }
        }
    }

    public void GetFriendslist()
    {
        string apiMutualFriens = GlobalVariable.baseUrlArenaGO + "/nestjsApi/api/friend-list/mutual-friends";
        FriendList friendList = new FriendList();

        controler.GetDataRoutine(apiMutualFriens, controler.data.data.accessToken,
            (json) =>
            {
                Debug.Log("Data agora available loaded: " + json);
                try
                {
                    friendList = JsonUtility.FromJson<FriendList>(json);
                    if (friendList != null)
                    {
                        StartCoroutine(CreateFriendLists(friendList));

                    }

                }
                catch (System.Exception e)
                {
                    Debug.LogError("Failed to friendlis data: " + e.Message);

                }
            },
            (error) =>
            {
                Debug.LogError("Failed to friendlis data: " + error);
            }
        );
    }

    IEnumerator CreateFriendLists(FriendList friend)
    {
        friendsDataPrefabs.Clear();
        if (posFrienlist.childCount != 0)
        {
            for (int i = 0; i < posFrienlist.childCount; i++)
            {
                Destroy(posFrienlist.GetChild(i).gameObject);
            }
        }

        for (int ii = 0; ii < friend.data.users.Length; ii++)
        {
            int i = ii;
            GameObject go = Instantiate(frienlist, posFrienlist);
            go.name = friend.data.users[i].name;
            FriendListDataPrefab friendListDataPrefab = go.GetComponent<FriendListDataPrefab>();

            string urlImg = friend.data.users[i].profileImage.ToString();

            Sprite avtr = null;

            controler.GetSpriteFromURL(urlImg, (downloadedSprite) =>
            {
                // Callback ini akan berjalan setelah download selesai
                if (downloadedSprite != null)
                {
                    avtr = downloadedSprite;
                }
                else
                {
                    avtr = defaultAvatar;
                    Debug.LogWarning("Gagal mendapatkan sprite dari URL.");
                }
            });

            yield return new WaitUntil(() => avtr != null);

            string nickName = friend.data.users[i].name;

            friendListDataPrefab.SetFriendlist(avtr, nickName);

            friendListDataPrefab.btnInvite.onClick.AddListener(() =>
            {

                string _avatarUrl = "";
                if (controler.data.data.profile.avatarUrl != null || controler.data.data.profile.avatarUrl != "")
                {
                    _avatarUrl = controler.data.data.profile.avatarUrl;
                }
                InviteFriend(friendListDataPrefab.nickNameFriendForInvite, controler.data.data.profile.username, _avatarUrl);
                // GlobalVariable.STATUS = Status.INVITING;
            });


            friendsDataPrefabs.Add(go);
        }

        RtmChannelManager.instant.PrintOnlineUsers();
    }

    void InviteFriend(string friendUsername, string myname, string avatarurl)
    {
        string gm = "";
        if (GlobalVariable.gamemode == GlobalVariable.GAMEMODE.FACEMODE)
            gm = "facemode";
        else if (GlobalVariable.gamemode == GlobalVariable.GAMEMODE.SMARTWACTH)
            gm = "smartwatch";
        else
            gm = "";
        rtmChannelManager.InviteFriend(friendUsername, myname, avatarurl, gm);
    }


    public void IncomingInvite(string inviting, string invited, string avatarUrl, string gamemode)
    {

        GlobalVariable.GAMEMODE gm = GlobalVariable.GAMEMODE.FACEMODE;

        if (gamemode == "facemode")
            gm = GlobalVariable.GAMEMODE.FACEMODE;
        else if (gamemode == "smartwatch")
            gm = GlobalVariable.GAMEMODE.SMARTWACTH;
        else
            gm = GlobalVariable.GAMEMODE.VOICEMODE;


        if (GlobalVariable.gamemode != gm)
            return;

        if (controler.data.data.profile.username == invited)
        {
            if (controler.data.data.profile.username == invited)
            {
                if (GlobalVariable.STATUS == Status.STANDBY)
                {
                    Debug.Log($"{inviting} mengundang sudah difilter {invited} urlAvatar {avatarUrl}");
                    StartCoroutine(INVITE.Instance.SetInviteOpen(inviting, "invite you to play", avatarUrl));
                    GlobalVariable.STATUS = Status.INVITED;
                }
            }

        }
    }



    /// <summary>
    /// //=====================================================
    /// </summary>

    [System.Serializable]
    public class FriendUser
    {
        public string uid;
        public string name;
        public string profileImage;
        public bool isOnline;
        public bool isMutual;
    }

    [System.Serializable]
    public class FriendData
    {
        public FriendUser[] users;
        public bool hasMore;
        public string lastUserId;
    }

    [System.Serializable]
    public class FriendList
    {
        public bool success;
        public FriendData data;
        public string message;
    }

}
