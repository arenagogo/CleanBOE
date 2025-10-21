using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class FriendRequestManager : MonoBehaviour
{
    public Transform posListFriendReq;
    public GameObject prefabFriendReq;  
    public SimpleAgoraController_Unified controler;
    public RtmChannelManager rtmChannelManager;
    public Sprite defaultAvatar;
    public Toggle toggleFriendReq;

    public CanvasGroup[] hide;
    public CanvasGroup show;


    public void OpenFriendRequest()
    {
        if (toggleFriendReq.isOn)
        {
            GetFriendRequest();
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





    public void GetFriendRequest()
    {
        if(posListFriendReq.childCount > 0)
        {
            for (int i = posListFriendReq.childCount - 1; i >= 0; i--)
            {
                Destroy(posListFriendReq.GetChild(i).gameObject);
            }
        }

        string apiFriendReq = GlobalVariable.baseUrlArenaGO + "/nestjsApi/api/friend-list/followers";
        FollowersResponse friendRequest = new FollowersResponse();
        controler.GetDataRoutine(apiFriendReq, controler.data.data.accessToken,
            (json) =>
            {
                Debug.Log("Data agora available loaded: " + json);
                try
                {
                    friendRequest = JsonUtility.FromJson<FollowersResponse>(json);
                    StartCoroutine(CreateFriendRequest(friendRequest));
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Failed to friend request data: " + e.Message);
                }
            },
            (error) => {
                Debug.LogError("Failed to friend request data: " + error);
            }
        );
    }



    IEnumerator CreateFriendRequest(FollowersResponse friendReq)
    {
        for (int x = 0; x < friendReq.data.users.Length; x++)
        {
            int i = x;

            if (friendReq.data.users[i].isMutual == true)
                continue;

            GameObject go = Instantiate(prefabFriendReq, posListFriendReq);
            go.transform.localScale = Vector3.one;
            UserList userList = go.GetComponent<UserList>();



            // panggil coroutine untuk ambil avatar
            yield return StartCoroutine(SpAvatar(friendReq.data.users[i].profileImage, (spAvtr) =>
            {
                bool statusOnline = false;
                foreach (string user in rtmChannelManager.onlineUser)
                {
                    if (user == friendReq.data.users[i].name)
                    {
                        statusOnline = true;
                        break;
                    }
                }
                Button btnAdd = go.GetComponent<UserList>().btnAddFriend;
                btnAdd.onClick.AddListener(() => {
                    AddFriend(friendReq.data.users[i].uid);
                    btnAdd.interactable = false;
                });
                userList.SetUserlist(spAvtr, friendReq.data.users[i].name, statusOnline);
            }));

            yield return null;
        }
    }

    void AddFriend(string uid)
    {
        string apiAddfriend = GlobalVariable.baseUrlArenaGO + "/nestjsApi/api/friend-list/follow/" + uid;
        controler.PostJson(apiAddfriend, controler.data.data.accessToken, "",
             (json) =>
             {
                 Debug.Log("Sukses addfriend: " + json);
             },
             (err) => Debug.LogError("Gagal addfriend: " + err)
         );
    }


    IEnumerator SpAvatar(string url, System.Action<Sprite> callback)
    {
        Sprite avtr = null;
        controler.GetSpriteFromURL(url, (downloadedSprite) =>
        {
            if (downloadedSprite != null)
                avtr = downloadedSprite;
            else
                avtr = defaultAvatar;
        });

        // tunggu sampai avtr terisi
        yield return new WaitUntil(() => avtr != null);
        callback(avtr);
    }

    /// <summary>
    /// // Response model for followers API
    /// </summary>

    [System.Serializable]
    public class FollowersResponse
    {
        public bool success;
        public FollowersData data;
        public string message;
    }

    [System.Serializable]
    public class FollowersData
    {
        public UserData[] users;
        public bool hasMore;
        public string lastUserId;
    }

    [System.Serializable]
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
