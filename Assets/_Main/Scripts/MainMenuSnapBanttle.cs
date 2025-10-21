using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class MainMenuSnapBattle : MonoBehaviour
{
    public static MainMenuSnapBattle Instance;

    public SimpleAgoraController_Unified controler;

    public bool statusLogin = false;
    public CanvasGroupSetting groupSetting;

    //OnLogin Variable
    public Toggle toogleHome;
    public Toggle toogleAccount;
    public Toggle toogleTutor;
    public Toggle toogleHistory;
    public Toggle toogleSetting;
    public TextMeshProUGUI nickName;
    public Image avatar;

    public Transform posList;
    public GameObject historyPrefab;

    public HISTORY history;

    public Sprite defaultSprites;

    public CanvasGroup[] hideOnStartUp;
    public CanvasGroup[] showOnStartUp;



    private void Awake()
    {
        Instance = this;
    }

    public void OnRestart()
    {
        foreach (CanvasGroup cvs in hideOnStartUp)
        {
            cvs.alpha = 0;
            cvs.interactable = false;
            cvs.blocksRaycasts = false;
        }

        foreach (CanvasGroup cvs in showOnStartUp)
        {
            cvs.alpha = 1;
            cvs.interactable = true;
            cvs.blocksRaycasts = true;
        }
    }

    [ContextMenu("SetNameCanvasgroup")]
    void SetNameCanvasgroup()
    {
        foreach (CanvasGroupManager mgr in groupSetting.cvsManager)
        {
            mgr.nameCanvasGroup = mgr.canvasGroup.name;
        }
    }


    public void ResetTokenExpired()
    {
        SHOWCanvasGroup("NoLogin");
    }



    public void PlayOrLogin()
    {

        if (statusLogin)
        {
            if (GlobalVariable.gamemode == GlobalVariable.GAMEMODE.SMARTWACTH)
            {
                OpenAccounMenu(true);
            }
            else
            {
                OpenGameModeOption();
            }
        }
        else
        {
            ResetTokenExpired();
        }
    }

    public void OpenHome()
    {
        if (!toogleHome.isOn)
            return;

        SHOWCanvasGroup("LogoOnMenu");
    }

    public void SuccessLogin()
    {
        SHOWCanvasGroup("OnLogin");
        SetProfile();
    }

    public void OpenAccounMenu()
    {
        if (!toogleAccount.isOn)
            return;



        if (statusLogin)
        {
            if (GlobalVariable.smartWatchConnected == false && GlobalVariable.gamemode == GlobalVariable.GAMEMODE.SMARTWACTH)
            {
                SHOWCanvasGroup("MenuTypeGame");
                return;
            }
            SHOWCanvasGroup("OnLogin");
            SetProfile();
        }
        else
        {
            SHOWCanvasGroup("NoLogin");
        }
    }

    public void OpenAccounMenu(bool onFinishGame)
    {
        toogleAccount.isOn = onFinishGame;

        if (statusLogin)
        {
            SHOWCanvasGroup("OnLogin");
            SetProfile();
        }
        else
        {
            SHOWCanvasGroup("NoLogin");
        }
    }

    public void OpenTutorial()
    {
        if (!toogleTutor.isOn)
            return;
        SHOWCanvasGroup("Tutorial");
    }

    public void OpenHistory()
    {
        if (!toogleHistory.isOn)
            return;

        //detaiHistoryCVS.DOFade(0, 0.5f);
        //detaiHistoryCVS.interactable = false;
        //detaiHistoryCVS.blocksRaycasts = false;

        HistoryCVS.DOFade(1, 0.5f);
        HistoryCVS.interactable = true;
        HistoryCVS.blocksRaycasts = true;

        if (statusLogin)
        {
            SHOWCanvasGroup("OnLoginHistory");
            CreateListHistory();
        }
        else
        {
            SHOWCanvasGroup("NologinHistory");
        }
    }

    public void OpenUrl(string url)
    {
        Application.OpenURL(url);
    }

    public void OpenSetting()
    {
        if (!toogleSetting.isOn)
            return;

        if (statusLogin)
        {
            SHOWCanvasGroup("Settings");
        }
        else
        {
            SHOWCanvasGroup("NologinHistory");
        }

    }

    public void OpenSettingButton()
    {
        toogleSetting.isOn = true;

        if (statusLogin)
        {
            SHOWCanvasGroup("Settings");
        }
        else
        {
            SHOWCanvasGroup("NologinHistory");
        }

    }

    public void OpenSettingExplan()
    {
        toogleSetting.isOn = false;
        SHOWCanvasGroup("Tutorial");
    }

    public void OpenGameModeOption()
    {
        SHOWCanvasGroup("MenuTypeGame");
    }



    public void CreateListHistory()
    {
        // string apiGetListHistory = "https://asia-southeast2-arenago-dev.cloudfunctions.net" + "/nestjsApi/api/snap-video-battles/my-scores";
        string apiGetListHistory = GlobalVariable.baseUrlArenaGO + "/nestjsApi/api/snap-video-battles/my-scores";

        controler.GetDataRoutine(apiGetListHistory, controler.data.data.accessToken,
            (json) =>
            {
                try
                {
                    history = JsonUtility.FromJson<HISTORY>(json);
                    SpawnHistory();
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Failed to parse agora history data: " + e.Message);
                }
            },
            (error) =>
            {
                Debug.LogError("Failed to agora history data: " + error);
            }
        );
    }

    void SetAura(int totalGames, int totalWins)
    {
        PlayerEmotionAnalyzer.instance.SetAura(totalGames, totalWins);
    }


    public void GetTotalScore(Action<int> onResult)
    {
        string apiGetListHistory = GlobalVariable.baseUrlArenaGO + "/nestjsApi/api/snap-video-battles/my-scores";
        controler.GetDataRoutine(apiGetListHistory, controler.data.data.accessToken,
            (json) =>
            {
                try
                {
                    HISTORY root = JsonConvert.DeserializeObject<HISTORY>(json);
                    int totalScore = 0;
                    foreach (var item in root.data)
                    {
                        totalScore += item.score;
                    }

                    // kirim balik hasil lewat callback
                    onResult?.Invoke(totalScore);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Failed to parse agora history data: " + e.Message);
                    onResult?.Invoke(0);
                }
            },
            (error) =>
            {
                Debug.LogError("Failed to agora history data: " + error);
                onResult?.Invoke(0);
            }
        );
    }


    // Fix for CS1061: 'List<MainMenuSnapBattle.Datum>' does not contain a definition for 'createdAt'
    // The error occurs because 'his.data' is a List<Datum>, not a single Datum.
    // You need to access an individual Datum from the list, e.g., his.data[0].createdAt or loop through the list.

    void SpawnHistory()
    {
        int totalGames = 0;
        int totalWins = 0;
        if (posList.childCount > 0)
        {
            for (int i = 0; i < posList.childCount; i++)
            {
                Destroy(posList.GetChild(i).gameObject);
            }
        }

        for (int ii = 0; ii < history.data.Count; ii++)
        {
            int i = ii;
            long lt = history.data[i].createdAt._seconds;
            int nano = history.data[i].createdAt._nanoseconds;

            totalGames++;
            if (history.data[i].status == "WIN")
            {
                totalWins++;
            }

            string dt = FirestoreTimestampToDateString(lt, nano);

            GameObject go = Instantiate(historyPrefab, posList);
            HistorySnapBattle historySnapBattle = go.GetComponent<HistorySnapBattle>();

            historySnapBattle.SetDataHistory(
                                   dt,
                                   history.data[i].status,
                                   history.data[i].score.ToString(),
                                   (i + 1).ToString()
             );


            Button btn = historySnapBattle.buttonDetail;
            btn.onClick.AddListener(() =>
            {

                DetailHistory(history.data[i].battleId);

            });

            // DetailHistory(history.data[i].battleId, dt, history.data[i].status, history.data[i].score.ToString(), i.ToString());
        }

        SetAura(totalGames, totalWins);
    }

    public HistoryDetail historyDetail;
    // public CanvasGroup detaiHistoryCVS;
    public CanvasGroup HistoryCVS;
    // public DetailHistoryResponse detail;
    private void DetailHistory(string id)
    {
        string apiGetLisDetailtHistory = GlobalVariable.baseUrlArenaGO + "/nestjsApi/api/snap-video-battles/" + id + "/history";

        controler.GetDataRoutine(apiGetLisDetailtHistory, controler.data.data.accessToken,
            (json) =>
            {
                Debug.Log("Data Detail  history available loaded: " + json);
                try
                {
                    DetailHistoryResponse detail = JsonUtility.FromJson<DetailHistoryResponse>(json);

                    if (detail != null && detail.data != null && detail.data.participants != null)
                    {
                        for (int i = 0; i < detail.data.participants.Length; i++)
                        {
                            if (detail.data.participants[i].username != controler.data.data.profile.username)
                            {
                                Sprite sp = null;
                                if (detail.data.participants[i].avatarUrl == null || detail.data.participants[i].avatarUrl == string.Empty)
                                {
                                    sp = defaultSprites;
                                }
                                else
                                {
                                    controler.GetSpriteFromURL(detail.data.participants[i].avatarUrl, (downloadedSprite) =>
                                    {
                                        // Callback ini akan berjalan setelah download selesai
                                        if (downloadedSprite != null)
                                        {
                                            sp = downloadedSprite;
                                        }
                                        else
                                        {
                                            sp = defaultSprites;
                                        }

                                        StartCoroutine(historyDetail.SetDetail(sp));


                                    });
                                }

                                // Ganti bagian ini di dalam fungsi DetailHistory()

                                // BENAR:
                                long lt = detail.data.userScores[0].createdAt._seconds;
                                int nano = detail.data.userScores[0].createdAt._nanoseconds;
                                string dt = FirestoreTimestampToDateString(lt, nano);

                                string nameRemote = "";

                                foreach (var remoteItem in detail.data.participants)
                                {
                                    if (remoteItem.username != controler.data.data.profile.username)
                                    {
                                        nameRemote = remoteItem.username;
                                    }
                                }

                                historyDetail.SetDetail(
                                    nameRemote, //nick remote
                                    dt, //date
                                    nameRemote, //nick remote
                                    detail.data.userScores[0].score.ToString(), // score
                                    detail.data.userScores[0].status // status
                                 );
                            }
                            else
                            {
                                long lt = detail.data.userScores[0].createdAt._seconds;
                                int nano = detail.data.userScores[0].createdAt._nanoseconds;
                                string dt = FirestoreTimestampToDateString(lt, nano);

                                string nameRemote = "Counter Interference";

                                foreach (var remoteItem in detail.data.participants)
                                {
                                    if (remoteItem.username != controler.data.data.profile.username)
                                    {
                                        nameRemote = remoteItem.username;
                                    }
                                }

                                historyDetail.SetDetail(
                                    nameRemote, //nick remote
                                    dt, //date
                                    nameRemote, //nick remote
                                    detail.data.userScores[0].score.ToString(), // score
                                    detail.data.userScores[0].status // status
                                 );

                                StartCoroutine(historyDetail.SetDetail(defaultSprites));
                            }
                        }

                        //detaiHistoryCVS.DOFade(1, 0.5f);
                        //detaiHistoryCVS.interactable = true;
                        //detaiHistoryCVS.blocksRaycasts = true;

                        HistoryCVS.DOFade(0, 0.5f);
                        HistoryCVS.interactable = false;
                        HistoryCVS.blocksRaycasts = false;
                    }
                    else
                    {
                        Debug.LogWarning("Detail history kosong / tidak sesuai JSON");
                    }



                }
                catch (System.Exception e)
                {
                    Debug.LogError("Failed to parse Detail history data: " + e.Message);
                }
            },
            (error) =>
            {
                Debug.LogError("Failed to Detail  history data: " + error);
            }
        );
    }

    public static DateTime FirestoreTimestampToDateTime(long seconds, int nanoseconds)
    {
        DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // 1 tick = 100 nanoseconds
        long ticksFromNanos = nanoseconds / 100;

        return epoch.AddSeconds(seconds).AddTicks(ticksFromNanos);
    }

    public static string FirestoreTimestampToDateString(long seconds, int nanoseconds)
    {
        DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        long ticksFromNanos = nanoseconds / 100;
        DateTime dateTime = epoch.AddSeconds(seconds).AddTicks(ticksFromNanos);

        return dateTime.ToLocalTime().ToString("dd/MM/yyyy");
    }


    public void SetProfile()
    {
        nickName.text = controler.data.data.profile.username;
        avatar.sprite = controler.avatarLocalSprite;
    }

    public CanvasGroup toggleMenu;

    public CanvasGroup panelBlock;
    public void SHOWCanvasGroup(string nameCVS)
    {
        panelBlock.DOFade(0.1f, 0.3f).OnComplete(() => { EnableToogel(); });
        panelBlock.interactable = true;
        panelBlock.blocksRaycasts = true;
        // toggleMenu.interactable = false;
        HIDECanvasGroup();

        foreach (CanvasGroupManager mgr in groupSetting.cvsManager)
        {
            if (mgr.nameCanvasGroup == nameCVS)
            {
                mgr.canvasGroup.DOFade(1, 0.5f);
                mgr.canvasGroup.interactable = true;
                mgr.canvasGroup.blocksRaycasts = true;
            }

        }
        Invoke("EnableToogel", 0.6f);
    }

    void EnableToogel()
    {
        panelBlock.DOFade(0, 0.3f).OnComplete(() =>
        {
            panelBlock.interactable = false;
            panelBlock.blocksRaycasts = false;
        });
    }

    public void HIDECanvasGroup()
    {
        foreach (CanvasGroupManager mgr in groupSetting.cvsManager)
        {
            mgr.canvasGroup.alpha = 0;
            mgr.canvasGroup.interactable = false;
            mgr.canvasGroup.blocksRaycasts = false;
        }
    }
    /// <summary>
    /// //===========================
    /// </summary>
    ///
    [Serializable]
    public class CanvasGroupSetting
    {
        public List<CanvasGroupManager> cvsManager;
    }

    [Serializable]
    public class CanvasGroupManager
    {
        public string nameCanvasGroup;
        public CanvasGroup canvasGroup;
        public Toggle toggle;
    }


    ////==========HYSTORY=============
    ///

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    [Serializable]
    public class CreatedAt
    {
        public int _seconds;
        public int _nanoseconds;
        public CreatedAt(int seconds, int nanoseconds)
        {
            _seconds = seconds;
            _nanoseconds = nanoseconds;
        }
    }
    [Serializable]
    public class Datum
    {
        public string battleId;
        public string roomId;
        public string userId;
        public int score;
        public string status;
        public CreatedAt createdAt;
        public int seconds;
        public int nanoseconds;
    }
    [Serializable]
    public class HISTORY
    {
        public bool success;
        public List<Datum> data;
        public string message;
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);

    ////==========DETAIL__HYSTORY=============
    [System.Serializable]
    public class DetailHistoryResponse
    {
        public bool success;
        public DetailData data;
        public string message;
    }

    [System.Serializable]
    public class DetailData
    {
        public UserScore[] userScores;
        public Participant[] participants;
        public string roomId;
        // public CreatedAt createdAt;
    }

    [System.Serializable]
    public class UserScore
    {
        public string battleId;
        public string roomId;
        public string userId;
        public int score;
        public string status;
        public CreatedAt createdAt;
    }


    [System.Serializable]
    public class Participant
    {
        public string userId;
        public string username;
        public string avatarUrl;
    }

}
