using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Unity.VisualScripting;

public class UserOnlineManager : MonoBehaviour
{
    public static UserOnlineManager instance;

    [Header("References")]
    public SimpleAgoraController_Unified controller;
    public GameObject userOnlinePrefab;
    public Transform posUserOnline;

    [Header("Colors")]
    public Color readyColor;
    public Color playingColor;
    public Color inviteColor;
    public Color addFriendColor;
    public Color requestFriendColor;

    [Header("Data")]
    public List<string> friendOnlineList = new List<string>();
    [SerializeField] private List<UserOnline> userOnlines = new List<UserOnline>();
    public List<string> followingList = new List<string>();
    public List<string> followersList = new List<string>();

    public GameObject noUserOnline;
    public CanvasGroup logoOnMenu;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        StartCoroutine(UpdateRefresUserOnlineList());
        // CheckUserOnline();
    }


    // --- Add User to Online List ---
    public void AddUserOnline(string nameUser)
    {
        if (nameUser == controller.data.data.profile.username)
            return;

        GameObject go = Instantiate(userOnlinePrefab, posUserOnline);
        go.name = nameUser;

        UserOnline userOnline = go.GetComponent<UserOnline>();
        userOnline.nickName.text = nameUser;

        userOnlines.Add(userOnline);

        noUserOnline.SetActive(userOnlines.Count == 0);


    }


    // --- Remove User from Online List ---
    public void RemoveUserOnline(string nameUser)
    {
        for (int i = userOnlines.Count - 1; i >= 0; i--)
        {
            if (userOnlines[i].nickName.text == nameUser)
            {
                Destroy(userOnlines[i].gameObject);
                userOnlines.RemoveAt(i);
            }
        }


        noUserOnline.SetActive(userOnlines.Count == 0);
    }

    IEnumerator UpdateRefresUserOnlineList()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            //  if (logoOnMenu.alpha == 1)
            RefresUserOnlineList();
        }

    }


    async void RefresUserOnlineList()
    {
        BroadCastMode(false);
        if (posUserOnline.childCount == 0)
        {
            noUserOnline.SetActive(true);
            return;
        }
        else
        {
            noUserOnline.SetActive(false);
        }

        await GetListFollowerAsync();
        await GetListFollowingAsync();
        await GetListFriendsAsync();


        foreach (var userOnline in userOnlines)
        {
            string username = userOnline.nickName.text;

            if (friendOnlineList.Contains(username))
            {
                userOnline.buttonText.text = "Invite";
                userOnline.buttonImage.color = inviteColor;
            }
            else if (followingList.Contains(username))
            {
                userOnline.buttonText.text = "Friend Request";
                userOnline.buttonImage.color = requestFriendColor;
                userOnline.actionButton.interactable = false;
            }
            else if (followersList.Contains(username))
            {
                userOnline.buttonText.text = "Accept Friend";
                userOnline.buttonImage.color = requestFriendColor;
                userOnline.actionButton.interactable = true;
                userOnline.actionButton.onClick.RemoveAllListeners();
                userOnline.actionButton.onClick.AddListener(() =>
                {
                    AddFriend(userOnline.uid);
                    userOnline.buttonText.text = "Invite";
                    userOnline.buttonImage.color = inviteColor;
                    BroadCastMode(true);
                    userOnline.actionButton.onClick.RemoveAllListeners();
                    userOnline.actionButton.onClick.AddListener(() =>
                    {
                        string _avatarUrl = "";
                        if (!string.IsNullOrEmpty(controller.data.data.profile.avatarUrl))
                        {
                            _avatarUrl = controller.data.data.profile.avatarUrl;
                        }
                        InviteFriend(userOnline.nickName.text, controller.data.data.profile.username, _avatarUrl);
                        BroadCastMode(true);
                    });
                });
            }
            else
            {
                userOnline.buttonText.text = "Add Friend";
                userOnline.buttonImage.color = addFriendColor;
                userOnline.actionButton.interactable = true;
                userOnline.actionButton.onClick.RemoveAllListeners();
                userOnline.actionButton.onClick.AddListener(() =>
                {
                    AddFriend(userOnline.uid);
                    userOnline.buttonText.text = "Friend Request";
                    userOnline.buttonImage.color = requestFriendColor;
                    BroadCastMode(true);
                });
            }
        }
    }


    // --- Broadcast current mode and playing status ---
    public void BroadCastMode(bool addDataFriend)
    {
        var data = new DataBroadcastStatus(
            controller.data.data.profile.username,
            GlobalVariable.gamemode.ToString(),
            GlobalVariable.onPlaying,
            controller.data.data.profile.avatarUrl,
            controller.data.data.uid,
            addDataFriend
        );

        string jsonData = JsonUtility.ToJson(data);
        RtmChannelManager.instant.BroadCastStatus(controller.data.data.profile.username, jsonData);
    }


    // --- Handle received broadcast ---
    public async void ReceivindBroadCastStatus(string username, string status)
    {
        Debug.Log($"[UserOnlineManager] Menerima BroadCastStatus dari {username}: {status}");

        DataBroadcastStatus dataStatus = JsonUtility.FromJson<DataBroadcastStatus>(status);

        // if (dataStatus.addDataFriend)
        // {
        //     StartCoroutine(UpdateRefresUserOnlineList());
        // }

        noUserOnline.SetActive(userOnlines.Count == 0);

        // Tunggu sampai followingList terisi
        // await GetListFollowingAsync();

        foreach (var userOnline in userOnlines)
        {
            if (userOnline.nickName.text != dataStatus.username)
                continue;

            userOnline.IsonlineStatus();
            userOnline.uid = dataStatus.uid;
            if (dataStatus.urlProfileImage != null && dataStatus.urlProfileImage != "")
            {
                Sprite avtr = null;
                controller.GetSpriteFromURL(dataStatus.urlProfileImage, (downloadedSprite) =>
                {
                    // Callback ini akan berjalan setelah download selesai
                    if (downloadedSprite != null)
                    {
                        avtr = downloadedSprite;
                        userOnline.icon.sprite = avtr;
                    }
                    else
                    {
                        Debug.LogWarning("Gagal mendapatkan sprite dari URL.");
                    }
                });
            }

            //Debug.Log($"[UserOnlineManager] Update status untuk {userOnline.nickName.text}: {dataStatus.status}, isPlaying: {dataStatus.isPlaying}");

            // Update Mode

            string mod = "";

            switch (GlobalVariable.gamemode)
            {
                case GlobalVariable.GAMEMODE.FACEMODE:
                    mod = "FACE MODE";
                    break;
                case GlobalVariable.GAMEMODE.SMARTWATCH:
                    mod = "SMARTWATCH";
                    break;
            }

            switch (dataStatus.status)
            {
                case "FACEMODE":
                    userOnline.modeText.text = "FACE MODE";
                    break;
                case "SMARTWATCH":
                    userOnline.modeText.text = "SMARTWATCH";
                    break;
            }

            // Update Status
            if (dataStatus.isPlaying)
            {
                userOnline.readyToPlayStatus.text = "Playing";
                userOnline.readyToPlayStatus.color = playingColor;
                userOnline.ReadyStatusIcon.color = playingColor;
                userOnline.actionButton.gameObject.SetActive(false);
            }
            else
            {
                userOnline.readyToPlayStatus.text = "Online";
                userOnline.readyToPlayStatus.color = readyColor;
                userOnline.ReadyStatusIcon.color = readyColor;
                userOnline.actionButton.gameObject.SetActive(true);
            }

            // Update button color dan teks
            if (friendOnlineList.Contains(username))
            {
                userOnline.buttonText.text = "Invite";
                userOnline.buttonImage.color = inviteColor;

                if (userOnline.modeText.text == mod)
                {
                    //Debug.Log($"[UserOnlineManager] Menyiapkan tombol undangan untuk {userOnline.nickName.text} {GlobalVariable.gamemode.ToString()}");
                    userOnline.actionButton.interactable = true;
                    userOnline.actionButton.onClick.RemoveAllListeners();
                    userOnline.actionButton.onClick.AddListener(() =>
                    {
                        string _avatarUrl = "";
                        if (!string.IsNullOrEmpty(controller.data.data.profile.avatarUrl))
                        {
                            _avatarUrl = controller.data.data.profile.avatarUrl;
                        }
                        InviteFriend(userOnline.nickName.text, controller.data.data.profile.username, _avatarUrl);
                        BroadCastMode(true);
                    });
                }
                else
                    userOnline.actionButton.interactable = false;
            }
            else if (followingList.Contains(username))
            {
                userOnline.buttonText.text = "Friend Request";
                userOnline.buttonImage.color = requestFriendColor;
                userOnline.actionButton.interactable = false;
            }
            else if (followersList.Contains(username))
            {
                userOnline.buttonText.text = "Accept Friend";
                userOnline.buttonImage.color = requestFriendColor;
                userOnline.actionButton.interactable = true;
                userOnline.actionButton.onClick.RemoveAllListeners();
                userOnline.actionButton.onClick.AddListener(() =>
                {
                    AddFriend(userOnline.uid);
                    userOnline.buttonText.text = "Invite";
                    userOnline.buttonImage.color = inviteColor;
                    BroadCastMode(true);
                    userOnline.actionButton.onClick.RemoveAllListeners();
                    userOnline.actionButton.onClick.AddListener(() =>
                    {
                        string _avatarUrl = "";
                        if (!string.IsNullOrEmpty(controller.data.data.profile.avatarUrl))
                        {
                            _avatarUrl = controller.data.data.profile.avatarUrl;
                        }
                        InviteFriend(userOnline.nickName.text, controller.data.data.profile.username, _avatarUrl);
                        BroadCastMode(true);
                    });
                });
            }
            else
            {
                userOnline.buttonText.text = "Add Friend";
                userOnline.buttonImage.color = addFriendColor;
                userOnline.actionButton.interactable = true;
                userOnline.actionButton.onClick.RemoveAllListeners();
                userOnline.actionButton.onClick.AddListener(() =>
                {
                    AddFriend(userOnline.uid);
                    userOnline.buttonText.text = "Friend Request";
                    userOnline.buttonImage.color = requestFriendColor;
                    BroadCastMode(true);

                });
            }

            break;
        }
    }

    async void CheckUserOnline()
    {
        await GetListFollowerAsync();
        await GetListFollowingAsync();
        await GetListFriendsAsync();
        noUserOnline.SetActive(userOnlines.Count == 0);
    }

    public RtmChannelManager rtmChannelManager;

    void InviteFriend(string friendUsername, string myname, string avatarurl)
    {
        string gm = "";
        if (GlobalVariable.gamemode == GlobalVariable.GAMEMODE.FACEMODE)
            gm = "facemode";
        else if (GlobalVariable.gamemode == GlobalVariable.GAMEMODE.SMARTWATCH)
            gm = "smartwatch";
        else
            gm = "";
        rtmChannelManager.InviteFriend(friendUsername, myname, avatarurl, gm);
    }


    async Task GetListFollowerAsync()
    {
        string apiUrl = GlobalVariable.baseUrlArenaGO + "/nestjsApi/api/friend-list/followers";
        var tcs = new TaskCompletionSource<bool>();
        controller.GetDataRoutine2(apiUrl, controller.data.data.accessToken,
            (json) =>
            {
                try
                {
                    var data = JsonUtility.FromJson<FollowingResponse>(json);
                    StartCoroutine(CreateFollowerList(data, tcs));
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                    tcs.TrySetResult(true);
                }
            },
            (err) => tcs.TrySetResult(true)
        );
        await tcs.Task;
    }

    async Task GetListFollowingAsync()
    {
        string apiUrl = GlobalVariable.baseUrlArenaGO + "/nestjsApi/api/friend-list/following";
        var tcs = new TaskCompletionSource<bool>();
        controller.GetDataRoutine2(apiUrl, controller.data.data.accessToken,
            (json) =>
            {
                try
                {
                    var data = JsonUtility.FromJson<FollowingResponse>(json);
                    StartCoroutine(CreateFollowingList(data, tcs));
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                    tcs.TrySetResult(true);
                }
            },
            (err) => tcs.TrySetResult(true)
        );
        await tcs.Task;
    }


    // --- Coroutine untuk isi followingList ---
    private IEnumerator CreateFollowingList(FollowingResponse following, TaskCompletionSource<bool> tcs)
    {
        followingList.Clear();

        for (int i = 0; i < following.data.users.Length; i++)
        {
            if (following.data.users[i].isMutual)
                continue;

            followingList.Add(following.data.users[i].name);
        }
        yield return null;
        tcs.TrySetResult(true);
    }

    private IEnumerator CreateFollowerList(FollowingResponse following, TaskCompletionSource<bool> tcs)
    {
        followersList.Clear();

        for (int i = 0; i < following.data.users.Length; i++)
        {
            if (following.data.users[i].isMutual)
                continue;

            followersList.Add(following.data.users[i].name);
        }
        yield return null;
        tcs.TrySetResult(true);
    }

    void AddFriend(string uid)
    {
        string apiAddfriend = GlobalVariable.baseUrlArenaGO + "/nestjsApi/api/friend-list/follow/" + uid;
        controller.PostJson(apiAddfriend, controller.data.data.accessToken, "",
             (json) =>
             {
                 Debug.Log("Sukses addfriend: " + json);
             },
             (err) => ErrorAddFriend()
         );
    }

    async Task GetListFriendsAsync()
    {
        string apiUrl = GlobalVariable.baseUrlArenaGO + "/nestjsApi/api/friend-list/mutual-friends";
        var tcs = new TaskCompletionSource<bool>();
        controller.GetDataRoutine2(apiUrl, controller.data.data.accessToken,
            (json) =>
            {
                try
                {
                    var data = JsonUtility.FromJson<FollowingResponse>(json);
                    StartCoroutine(CreateFriendList(data, tcs));
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                    tcs.TrySetResult(true);
                }
            },
            (err) => tcs.TrySetResult(true)
        );
        await tcs.Task;
    }
    private IEnumerator CreateFriendList(FollowingResponse friend, TaskCompletionSource<bool> tcs)
    {
        friendOnlineList.Clear();

        for (int i = 0; i < friend.data.users.Length; i++)
        {
            friendOnlineList.Add(friend.data.users[i].name);
        }
        yield return null;
        tcs.TrySetResult(true);
    }




    void ErrorAddFriend()
    {
        Loading.instance.HideLoading();
    }


    // --- Data Classes ---
    [Serializable]
    public class DataBroadcastStatus
    {
        public string username;
        public string status;
        public bool isPlaying;
        public string urlProfileImage;
        public string uid;

        public bool addDataFriend;

        public DataBroadcastStatus(string _username, string _status, bool _isPlaying, string _urlProfileImage, string _uid, bool _addDataFriend)
        {
            username = _username;
            status = _status;
            isPlaying = _isPlaying;
            urlProfileImage = _urlProfileImage;
            uid = _uid;
            addDataFriend = _addDataFriend;
        }
    }

    [Serializable]
    public class FollowingResponse
    {
        public bool success;
        public FollowersData data;
        public string message;
    }

    [Serializable]
    public class FollowersData
    {
        public UserData[] users;
        public bool hasMore;
        public string lastUserId;
    }

    [Serializable]
    public class UserData
    {
        public string uid;
        public string name;
        public string profileImage;
        public bool isOnline;
        public string lastSeen;
        public bool isMutual;
    }
}
