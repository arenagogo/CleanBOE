using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class SearchManager : MonoBehaviour
{
    public Transform posListUser;
    public GameObject prefabUser;
    public SimpleAgoraController_Unified controler;
    public TMP_InputField inputSearch;
    public Button btnSearch;

    public CanvasGroup cvsFriend, cvsSearch;
    public Toggle toggleFriend, toggleSearch;
    public Sprite defaultAvatar;
    public RtmChannelManager rtmChannelManager;

    public CanvasGroup cvsLoading;

    public CanvasGroup[] hide;
    public CanvasGroup show;
    public Toggle toggleFriendReq;

    public void OpenFriendRequest()
    {
        if (toggleFriendReq.isOn)
        {
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



    private void Start()
    {
        btnSearch.onClick.AddListener(() => {
            if (inputSearch.text.Length >= 3)
            {
                SearchUser(inputSearch.text);
            }
        });
    }


    public void SearchUser(string username)
    {
        string apiMutualFriens = GlobalVariable.baseUrlArenaGO + "/nestjsApi/api/friend-list/search-people?q=" +username+"&page=1&limit=20";

        ListSearchUser listSearchUser = new ListSearchUser();

        controler.GetDataRoutine(apiMutualFriens, controler.data.data.accessToken,
            (json) =>
            {
                Debug.Log("Data agora available loaded: " + json);
                try
                {
                    listSearchUser  = JsonUtility.FromJson<ListSearchUser>(json);
                    if (listSearchUser != null)
                    {
                      
                        StartCoroutine(CreateListUser(listSearchUser));
                    }

                }
                catch (System.Exception e)
                {
                    Debug.LogError("Failed to friendlis data: " + e.Message);

                }
            },
            (error) => {
                Debug.LogError("Failed to friendlis data: " + error);
            }
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



    IEnumerator CreateListUser(ListSearchUser listUser)
    {
        cvsLoading.alpha = 1;
        if (posListUser.childCount != 0)
        {
            for (int i = 0; i < posListUser.childCount; i++)
            {
                Destroy(posListUser.GetChild(i).gameObject);
            }
        }
        for (int x = 0; x < listUser.data.users.Length; x++)
        {
            int i = x;
            GameObject go = Instantiate(prefabUser, posListUser);
            go.transform.localScale = Vector3.one;
            UserList userList = go.GetComponent<UserList>();

            // panggil coroutine untuk ambil avatar
            yield return StartCoroutine(SpAvatar(listUser.data.users[i].profileImage, (spAvtr) =>
            {
                bool statusOnline = false;
                foreach (string user in rtmChannelManager.onlineUser)
                {
                    if (user == listUser.data.users[i].username)
                    {
                        statusOnline = true;
                        break;
                    }
                }



                Button btnAdd = go.GetComponent<UserList>().btnAddFriend;

                if(listUser.data.users[i].relationshipStatus == "mutual")
                {
                    btnAdd.interactable = false;
                    btnAdd.GetComponentInChildren<TextMeshProUGUI>().text = "FRIENDS";
                }
                else if (listUser.data.users[i].relationshipStatus == "none")
                {
                    btnAdd.GetComponentInChildren<TextMeshProUGUI>().text = "ADD FRIEND";
                }
                else if (listUser.data.users[i].relationshipStatus == "following")
                {
                    btnAdd.interactable = false;
                    btnAdd.GetComponentInChildren<TextMeshProUGUI>().text = "REQUESTED";
                }
                else if (listUser.data.users[i].relationshipStatus == "follower")
                {
                    btnAdd.GetComponentInChildren<TextMeshProUGUI>().text = "ACCEPT";
                }


                btnAdd.onClick.AddListener(() => {
                    AddFriend(listUser.data.users[i].uid);
                    btnAdd.interactable = false;
                    btnAdd.GetComponentInChildren<TextMeshProUGUI>().text = "REQUESTED";
                });
                userList.SetUserlist(spAvtr, listUser.data.users[i].username, statusOnline);
            }));

            yield return null;
        }
        cvsLoading.alpha = 0;
    }


    void AddFriend(string uid)
    {
        string apiAddfriend = GlobalVariable.baseUrlArenaGO + "/nestjsApi/api/friend-list/follow/" + uid;
        controler.PostJson(apiAddfriend, controler.data.data.accessToken, "",
             (json) =>
             {
                Debug.Log("Sukses addfriend: " + json);
             },
             (err) => ErrorAddFriend()
         );
    }

    void ErrorAddFriend()
    {
        Loading.instance.HideLoading();
    }

    /// <summary>
    /// /===============================
    /// </summary>


    [System.Serializable]
    public class ListSearchUser
    {
        public bool success;
        public Data data;
        public string message;
    }

    [System.Serializable]
    public class Data
    {
        public User[] users;
        public Pagination pagination;
    }

    [System.Serializable]
    public class User
    {
        public string uid;
        public string name;
        public string username;
        public string profileImage;
        public bool isOnline;
        public string relationshipStatus;
    }

    [System.Serializable]
    public class Pagination
    {
        public int currentPage;
        public int totalPages;
        public int totalItems;
        public bool hasNext;
        public bool hasPrev;
    }

}
