using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Agora.Rtc;                 // Agora v4.x
using Agora.Rtc.LitJson;         // Untuk JsonMapper.ToJson
// Jika kamu punya model ini di project
using ArenaGo.Models;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class SimpleAgoraController_Unified : MonoBehaviour
{
    [Header("Konfigurasi Agora")]
    [SerializeField] private string _appID = "";
    [SerializeField] private string _tokenJwt = "";

    [Header("UI / Panel")]
    public TMP_InputField nicknameInput;
    public TMP_InputField roomNameInput;

    [Tooltip("RawImage untuk preview lokal (kamera + OpenCV). Agora tidak membuka kamera sendiri.")]
    public RawImage unifiedLocalPreview;
    public AspectRatioFitter unifiedPreviewFitter;

    [Tooltip("Panel untuk video REMOTE (tetap pakai VideoSurface).")]
    public GameObject remoteVideoPanel;

    public Button joinButton;
    public Button leaveButton;
    public TMP_Text logText;

    public CanvasGroup cvsPanelLobby;
    public CanvasGroup cvsPanelViCall;
    public CanvasGroup monitorRemote;

    public TextMeshProUGUI nickRemote;
    public TextMeshProUGUI nickLocal;

    // ===== Backend (sesuaikan endpointmu) =====
    [Header("Backend Token")]
    //  [SerializeField] private string apiUrlx = "https://asia-southeast2-arenago-a80c0.cloudfunctions.net/nestjsApi/api/auth/login";
    // [SerializeField] private string apiUrlAgoraTokenChanelx = "https://asia-southeast2-arenago-dev.cloudfunctions.net/nestjsApi/api/agora/account-token";
    // [SerializeField] private string apiUrlAgoraGetAllRoom = "https://asia-southeast2-arenago-dev.cloudfunctions.net/nestjsApi/api/snap-video-battles";

    [SerializeField] private string _tokenLogin;
    [SerializeField] private string token_Agora_Chanel;

    public LoginResponse data;           // dari ArenaGo.Models
    public TokenResponseAGora dataTokenAgora;

    // ===== Agora State =====
    private IRtcEngine _rtc;
    internal bool joinSuccess;
    private uint _remoteUid;
    private Coroutine joinTimeoutRoutine;

    public DataRoom dataRoom;

    public CanvasGroup animaVS;
    public TextMeshProUGUI nickNameRemoteVS;
    public TextMeshProUGUI nickNameLocalVS;

    public Sprite avatarLocalSprite;
    public CanvasGroup panelRoomAvailable;
    public HeartRateEstimator_Unified heartRateEstimator_Unified;

    public static event Action<int> OnScoreChanged;
    private int score;



    // =================== UNITY LIFECYCLE ===================
    void Start()
    {
        SetupUI();
        InitEngine();
    }


    void OnDestroy()
    {
        if (_rtc != null)
        {
            try { LeaveChannel(true); } catch { }
            _rtc.Dispose();
            _rtc = null;
        }
    }


    public void FoceQuit()
    {
        if (_rtc != null)
        {
            try { LeaveChannel(true); } catch { }
            _rtc.Dispose();
            _rtc = null;
        }
    }

    public int Score
    {
        get => score;
        private set
        {
            score = value;
            // Panggil event jika ada yang subscribe
            OnScoreChanged?.Invoke(score);
        }
    }

    public void TotalScore(int amount)
    {
        Score = amount;
    }



    // =================== PUBLIC API (dipanggil OpenCV) ===================
    /// <summary>
    /// Dipanggil dari script OpenCV untuk mendorong (push) frame RGBA (width x height) ke Agora.
    /// Panggil ini setelah kamu render/rotate/mirror di sisi OpenCV agar **tidak konflik kamera**.
    /// </summary>
    //public void PushExternalVideoFrame(byte[] rgba, int width, int height)
    //{
    //    if (_rtc == null || !joinSuccess || rgba == null || rgba.Length < width * height * 4) return;

    //    var frame = new ExternalVideoFrame
    //    {
    //        type      = VIDEO_BUFFER_TYPE.VIDEO_BUFFER_RAW_DATA,       // v4.x enum top-level
    //        format    = VIDEO_PIXEL_FORMAT.VIDEO_PIXEL_RGBA,
    //        buffer    = rgba,
    //        stride    = width,
    //        height    = height,
    //        rotation  = 0,                                             // sudah kamu handle di sisi OpenCV
    //        timestamp = (long)(Time.realtimeSinceStartup * 1000)
    //    };

    //    int rc = _rtc.PushVideoFrame(frame);
    //    if (rc != 0)
    //    {
    //        // Uncomment untuk debug:
    //        // Debug.LogWarning($"[Agora] PushVideoFrame rc={rc}");
    //    }
    //}

    public void PushExternalVideoFrame(byte[] rgba, int width, int height)
    {
        if (rgba == null || rgba.Length != width * height * 4)
        {
            Debug.LogError($"[PushExternalVideoFrame] Invalid buffer: {rgba?.Length}, expected {width * height * 4}");
            return;
        }

        ExternalVideoFrame frame = new ExternalVideoFrame
        {
            type = VIDEO_BUFFER_TYPE.VIDEO_BUFFER_RAW_DATA,
            format = VIDEO_PIXEL_FORMAT.VIDEO_PIXEL_RGBA,
            buffer = rgba,
            stride = width,
            height = height,
            cropLeft = 0,
            cropTop = 0,
            cropRight = 0,
            cropBottom = 0,
            rotation = 0,
            timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds()
        };

        int result = _rtc.PushVideoFrame(frame);
        // Debug.Log($"[PushExternalVideoFrame] Pushed {width}x{height}, buffer={rgba.Length}, result={result}");
    }


    /// <summary>
    /// Opsional: dipanggil script OpenCV untuk menyamakan aspect ratio RawImage preview lokal.
    /// </summary>
    public void SetUnifiedPreviewAspect(float w, float h)
    {
        if (unifiedPreviewFitter != null && h > 0f)
            unifiedPreviewFitter.aspectRatio = w / h;
    }


    // =================== AGORA SETUP ===================
    private void InitEngine()
    {
        _rtc = RtcEngine.CreateAgoraRtcEngine();

        var ctx = new RtcEngineContext
        {
            appId = _appID,
            channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_COMMUNICATION,
            audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT
        };
        _rtc.Initialize(ctx);

        var handler = new UserEventHandler(this);
        _rtc.InitEventHandler(handler);

        _rtc.EnableAudio();
        _rtc.EnableVideo();
        _rtc.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);

        // ✅ FIXED signature (4 args)
        _rtc.SetExternalVideoSource(
            true,
            false,
            EXTERNAL_VIDEO_SOURCE_TYPE.VIDEO_FRAME,
            new SenderOptions()
        );

        var config = new VideoEncoderConfiguration
        {
            dimensions = new VideoDimensions(640, 480),
            frameRate = 30,
            bitrate = 400,
            orientationMode = ORIENTATION_MODE.ORIENTATION_MODE_ADAPTIVE,
        };
        _rtc.SetVideoEncoderConfiguration(config);



        // SetTokenAgoraChanel();
    }


    private void SetupUI()
    {
        // if (joinButton) joinButton.onClick.AddListener(SetTokenAgoraChanel);
        // if (leaveButton) leaveButton.onClick.AddListener(LeaveChannel);
        if (remoteVideoPanel) remoteVideoPanel.SetActive(false);
    }

    // =================== JOIN / LEAVE ===================
    private void SetTokenAgoraChanel()
    {
        // Login ke backendmu untuk dapatkan accessToken → lanjut minta token channel
        GetDataAcount(_tokenLogin);

    }

    public void GetDataAcount(string tokenParams)
    {
        string apiUrl = GlobalVariable.baseUrlArenaGO + "/nestjsApi/api/auth/login";
        LoginWithCustomToken(
            apiUrl,
            tokenParams,
            (json) =>
            {
                try
                {
                    data = JsonUtility.FromJson<LoginResponse>(json);

                    GetSpriteFromURL(data.data.profile.avatarUrl, (downloadedSprite) =>
                    {
                        // Callback ini akan berjalan setelah download selesai
                        if (downloadedSprite != null)
                        {
                            Debug.Log("Sprite berhasil di-download! remote");
                            if (avatarLocal != null)
                            {
                                avatarLocal.sprite = downloadedSprite;
                                avatarLocalUI.sprite = downloadedSprite;
                                GlobalVariable.avatarLocal = downloadedSprite;
                            }
                        }
                        else
                        {
                            Debug.LogError("Gagal mendapatkan sprite dari URL.");
                        }
                    });

                    GlobalVariable.email_local = data.data.email;
                    Debug.Log(data.data.email);
                    MainMenuSnapBattle.Instance.statusLogin = true;
                    MainMenuSnapBattle.Instance.CreateListHistory();
                    // MainMenuSnapBattle.Instance.SetProfile();
                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to parse login response: " + e.Message);
                }
            },
            (err) =>

            ErrorLogin(err)
        );
    }

    public Sprite defaultAvatar;

    public IEnumerator GetdataAccount(bool xx)
    {
        yield return new WaitUntil(() => data != null && data.data != null);

        GetSpriteFromURL(data.data.profile.avatarUrl, (downloadedSprite) =>
        {
            // Callback ini akan berjalan setelah download selesai
            if (downloadedSprite != null)
            {
                avatarLocalSprite = downloadedSprite;
                Debug.Log("Sprite berhasil di-download! remote");
                if (avatarLocal != null)
                {
                    avatarLocal.sprite = downloadedSprite;
                    avatarLocalUI.sprite = downloadedSprite;
                    GlobalVariable.avatarLocal = downloadedSprite;
                }
            }
            else
            {
                Debug.Log("Gagal mendapatkan sprite dari URL.");
                avatarLocalSprite = defaultAvatar;
                avatarLocal.sprite = defaultAvatar;
                avatarLocalUI.sprite = defaultAvatar;
            }
        });

        yield return new WaitUntil(() => avatarLocalSprite != null);

        GlobalVariable.email_local = data.data.email;
        Debug.Log(data.data.email);
        MainMenuSnapBattle.Instance.statusLogin = true;
        MainMenuSnapBattle.Instance.SetProfile();
        RtmChannelManager.instant.SetChanelRTM(data.data.profile.username);
        FriendListsManager.instance.GetFriendslist();
        MainMenuSnapBattle.Instance.CreateListHistory();


        MainMenuSnapBattle.Instance.GetTotalScore((total) =>
        {
            TotalScore(total);
        });



        if (xx)
            MainMenuSnapBattle.Instance.SuccessLogin();
    }

    void ErrorLogin(string err)
    {
        Debug.LogError("Failed Login " + err);
        MainMenuSnapBattle.Instance.statusLogin = false;
        Loading.instance.HideLoading();
    }

    public void RefreshAllDataRoom()
    {
        // StartCoroutine(RefreshAllData());
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

    public string _roomName = "";

    private void JoinRoomTokenChanel(string nameRoom)
    {
        //bool avail = false;

        //foreach(var dt in dataRoom.data)
        //{
        //    if(dt.roomName == nameRoom)
        //    {
        //        avail = true; break;
        //    }
        //}

        //if (avail == false)
        //{
        //    Debug.Log($"romm {nameRoom} not available");
        //    return;
        //}
        string apiUrlAgoraTokenChanel = GlobalVariable.baseUrlArenaGO + "/nestjsApi/api/agora/account-token";

        var tokenRequest = new GetTokenChanel(nameRoom, data.data.profile.username, 3600);
        string rawBody = JsonMapper.ToJson(tokenRequest);

        PostJson(apiUrlAgoraTokenChanel, data.data.accessToken, rawBody,
            (json) =>
            {
                dataTokenAgora = JsonUtility.FromJson<TokenResponseAGora>(json);
                token_Agora_Chanel = dataTokenAgora.data.token;
                JoinChannel(token_Agora_Chanel, nameRoom, data.data.profile.username);
                GlobalVariable.nickNameAgora = data.data.profile.username;
                AnimateVSOnFighr();
            },
            (err) => Debug.LogError("Gagal minta token channel: " + err)
        );
    }

    public void JoinBattle(string id)
    {
        string urlApiLevaeChanel = GlobalVariable.baseUrlArenaGO + "/nestjsApi/api/snap-video-battles/" + id + "/join";
        HttpHelper.PostNoBodyAsync(urlApiLevaeChanel, data.data.accessToken);
        Debug.Log("Join battle " + id);
        //  Invoke(nameof(Addscore), 5f);
    }


    private void CreateRoomTokenChanel()
    {
        string apiUrlAgoraTokenChanel = GlobalVariable.baseUrlArenaGO + "/nestjsApi/api/agora/account-token";
        var tokenRequest = new GetTokenChanel(_roomName, data.data.profile.username, 3600);
        string rawBody = JsonMapper.ToJson(tokenRequest);

        PostJson(apiUrlAgoraTokenChanel, data.data.accessToken, rawBody,
            (json) =>
            {
                dataTokenAgora = JsonUtility.FromJson<TokenResponseAGora>(json);
                token_Agora_Chanel = dataTokenAgora.data.token;
                JoinChannel(token_Agora_Chanel, _roomName, data.data.profile.username);
                GlobalVariable.nickNameAgora = data.data.profile.username;
                //  CreateListRoom();
            },
            (err) => Debug.LogError("Gagal minta token channel: " + err)
        );
    }

    public void JoinChanelOnInvite(string roomName, string userName, string _battleId)
    {

        CretaeRoom(roomName, _battleId);
    }

    public void JoinChannel(string _tokenAgoraChanel, string roomName, string nickName)
    {
        if (nickLocal) nickLocal.text = nickName;
        nickNameLocalVS.text = nickName;
        Log($"Bergabung ke channel: {roomName} sebagai {nickName}...");
        joinSuccess = false;

        if (joinTimeoutRoutine != null) StopCoroutine(joinTimeoutRoutine);
        joinTimeoutRoutine = StartCoroutine(JoinTimeout());

        // Penting: JANGAN meng-setup VideoSurface untuk local video.
        // Local preview diisi oleh RawImage (unifiedLocalPreview) dari script OpenCV.
        _rtc.JoinChannelWithUserAccount(_tokenAgoraChanel, roomName, nickName);

        // Tampilkan panel video call
        SwitchPanel(cvsPanelViCall, cvsPanelLobby);
        INVITE.Instance.JoinChaelInvite();
    }

    public void LeaveChannel(bool isNormal)
    {
        if (isNormal)
        {
            if (battleId == "")
            {
                Log("Kamu belum bergabung di room manapun.");
                return;
            }

            GlobalVariable.STATUS = Status.STANDBY;

            Log("Meninggalkan channel...");
            _rtc.LeaveChannel();

            // Matikan VideoSurface remote (jika ada)
            if (remoteVideoPanel != null)
            {
                var vs = remoteVideoPanel.GetComponent<VideoSurface>();
                if (vs != null) vs.SetEnable(false);
                remoteVideoPanel.SetActive(false);
            }

            SwitchPanel(cvsPanelLobby, cvsPanelViCall);

            FinishRoom();
        }
        else
        {
            _rtc.LeaveChannel();
            LeaveChanelUser();
        }
    }



    public void LeaveChanelUser()
    {
        ResetGlobalVariable();
        RtmChannelManager.instant.LeaveRtmChannel();
    }

    void ResetGlobalVariable()
    {
        GlobalVariable.avatarObject = null;
        GlobalVariable.name = "";
        GlobalVariable.token = "";
        GlobalVariable.gender = "";
        GlobalVariable.prefabname = "";
        // public static bool isMobileBrowser = false;
        GlobalVariable.role = 0;
        GlobalVariable.boothCode = "";

        //  public static UserData dataUSer;
        GlobalVariable.baseUrl = "";
        GlobalVariable.walletCoin = 0;
        GlobalVariable.nickNameAgora = "";
        GlobalVariable.nickNameAgoraRemote = "";
        GlobalVariable.heartRate = 0;

        // public static string baseUrlArenaGO = "https://asia-southeast2-arenago-dev.cloudfunctions.net";
        // public static string baseUrlArenaGO = "https://asia-southeast2-arenago-a80c0.cloudfunctions.net";

        GlobalVariable.smartWatchConnected = false;

        GlobalVariable.maxHeartRate = 100f;

        GlobalVariable.avatarLocal = null;
        GlobalVariable.avatarRemote = null;

        GlobalVariable.email_local = "";

        GlobalVariable.gamemode = GlobalVariable.GAMEMODE.FACEMODE;

        GlobalVariable.BPM = 0;
        GlobalVariable.onPlaying = false;




        GlobalVariable.STATUS = Status.STANDBY;

    }

    public void FinishRoom()
    {
        if (GlobalVariable.STATUS == Status.PLAYING)
        {
            //GlobalVariable.STATUS = Status.STANDBY;
            GameScoreManager.instance.GameEndedRemoteLeave();
            //CardManager.instance.ResetCardOnUserLeave();

        }

        if (isMaster)
        {
            string urlApiLevaeChanel = GlobalVariable.baseUrlArenaGO + "/nestjsApi/api/snap-video-battles/" + battleId + "/finish";
            HttpHelper.PostNoBodyAsync(urlApiLevaeChanel, data.data.accessToken);
            isMaster = false;
        }

    }



    public void Addscore(int score, string status)
    {
        AddScore scr = new AddScore(score, status);
        string jsn = JsonMapper.ToJson(scr);
        string urlApi = GlobalVariable.baseUrlArenaGO + "/nestjsApi/api/snap-video-battles/" + battleId + "/score";
        Debug.Log("Addscore " + jsn + " and " + battleId);
        PostJson(urlApi, data.data.accessToken, jsn,
            (json) =>
            {
                Debug.Log("Addscore " + json);
            },
            (err) => Debug.LogError("Gagal minta token channel: " + err)
        );

        Score += score;
    }

    // =================== REMOTE VIDEO ===================
    public void SetupRemotePanel(GameObject panel, uint uid, string channelId = "")
    {
        if (panel == null) return;
        panel.SetActive(true);

        var videoSurface = panel.GetComponent<VideoSurface>();
        if (videoSurface == null) videoSurface = panel.AddComponent<VideoSurface>();

        // Tampilkan video user remote
        videoSurface.SetForUser(uid, channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
        videoSurface.SetEnable(true);
    }

    private IEnumerator JoinTimeout()
    {
        yield return new WaitForSeconds(10f);
        if (!joinSuccess)
        {
            Log("Gagal bergabung: timeout. Cek jaringan/token/AppID.");
            LeaveChannel(true);
        }
    }

    // =================== UI HELPERS ===================
    private void SwitchPanel(CanvasGroup show, CanvasGroup hide)
    {
        if (show != null)
        {
            show.alpha = 1f;
            show.interactable = true;
            show.blocksRaycasts = true;
        }
        if (hide != null)
        {
            hide.alpha = 0f;
            hide.interactable = false;
            hide.blocksRaycasts = false;
        }
    }

    public void Log(string message)
    {
        Debug.Log(message);
        if (logText) logText.text = message + "\n" + logText.text;
    }

    // =================== EVENT HANDLER ===================
    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly SimpleAgoraController_Unified _c;

        internal UserEventHandler(SimpleAgoraController_Unified c)
        {
            _c = c;
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            _c.joinSuccess = true;
            _c.Log($"Join OK: {connection.channelId} uid={connection.localUid}");
            _c.CreateRtcDataStream(true, true); // reliable+ordered untuk chat
        }

        // Replace the incorrect OnStreamMessage override in UserEventHandler with the correct signature.
        // The correct signature (from your type signatures) is:
        // public virtual void OnStreamMessage(RtcConnection connection, uint remoteUid, int streamId, byte[] data, ulong length, ulong sentTs)

        public override void OnStreamMessage(
            RtcConnection connection, uint remoteUid, int streamId, byte[] data, ulong length, ulong sentTs)
        {
            UserInfo info = new UserInfo();
            var result = _c._rtc.GetUserInfoByUid(remoteUid, ref info);
            string remoteNickname = info.userAccount;

            string text = System.Text.Encoding.UTF8.GetString(data, 0, (int)length);
            //  _c.OnChatMessage?.Invoke(remoteUid, remoteNickname + text);
            _c.OnChatMessage?.Invoke(remoteUid, $"<color=#FF8400>{remoteNickname}</color>\n<color=#000000>{text}</color>");
        }


        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {
            _c.Log("Left channel.");
            _c.LeaveChanelUser();
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _c.Log($"Remote UID joined: {uid}");
            _c._remoteUid = uid;
            _c.SetupRemotePanel(_c.remoteVideoPanel, uid, connection.channelId);

            // Ambil info akun remote (opsional)
            UserInfo info = new UserInfo();
            var result = _c._rtc.GetUserInfoByUid(uid, ref info);
            if (result == 0)
            {
                string remoteNickname = info.userAccount;
                _c.OnUserJoinedRemote(remoteNickname);
                _c.Log($"Remote info: UID {uid} → '{remoteNickname}'");
                // _c.animaVS.alpha = 1;
                // _c.FinishRoom();
            }
            else
            {
                _c.Log($"GetUserInfo error: {result}");
            }
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _c.Log($"Remote UID {uid} offline ({reason}).");
            if (_c.remoteVideoPanel) _c.remoteVideoPanel.SetActive(false);

            if (GlobalVariable.STATUS == Status.PLAYING)
            {
                _c.FinishRoom();
            }


        }

        public override void OnError(int err, string msg)
        {
            _c.Log($"[AGORA ERROR] {err} | {msg}");
            if (err == (int)ERROR_CODE_TYPE.ERR_JOIN_CHANNEL_REJECTED ||
                err == (int)ERROR_CODE_TYPE.ERR_INVALID_TOKEN)
            {
                _c.Log("Join gagal: periksa token/AppID/channel.");
                _c.LeaveChannel(true);
            }
        }
    }

    private void OnUserJoinedRemote(string nickNameUser)
    {
        animaVS.alpha = 1;
        if (monitorRemote != null)
        {
            monitorRemote.alpha = 1f;
            monitorRemote.interactable = true;
            monitorRemote.blocksRaycasts = true;
        }
        if (nickRemote) nickRemote.text = nickNameUser;
        nickNameRemoteVS.text = nickNameUser;
        AnimateVSOnFighr();
        SendChatText("papahjahat" + data.data.profile.avatarUrl);
        GlobalVariable.nickNameAgoraRemote = nickNameUser;
        GlobalVariable.STATUS = Status.PLAYING;
    }

    public GameObject waiting;

    public Image avatarLocal, avatarRemote;
    public Image avatarLocalUI, avatarRemoteUI;
    public CanvasGroup UI_profileRemote;
    public void AnimateVSOnFighr()
    {
        StartCoroutine(WaitingSet());
    }

    IEnumerator WaitingSet()
    {
        yield return new WaitUntil(() => GlobalVariable.nickNameAgoraRemote != null && GlobalVariable.nickNameAgoraRemote != "");
        if (isMaster)
        {
            TextMeshProUGUI text = waiting.GetComponentInChildren<TextMeshProUGUI>();
            text.text = GlobalVariable.nickNameAgoraRemote + " has entered";
            yield return new WaitForSeconds(1f);
            text.text = "Select a category and ask questions to " + GlobalVariable.nickNameAgoraRemote + " according to the category";
            yield return new WaitForSeconds(3f);
            waiting.GetComponent<CanvasGroup>().DOFade(0, 0.5f);
        }
        else
        {
            waiting.GetComponent<CanvasGroup>().alpha = 1f;
            TextMeshProUGUI text = waiting.GetComponentInChildren<TextMeshProUGUI>();
            text.text = $"wait for {GlobalVariable.nickNameAgoraRemote} to choose a category";
        }

        ShowCategory();
    }


    void ShowCategory()
    {
        AnimVS animVS = animaVS.GetComponent<AnimVS>();
        animVS.StarAnim();
        // Invoke("DelayShowCategory", 3f);

    }

    public void DelayShowCategory()
    {
        animaVS.alpha = 0;
        animaVS.interactable = false;
        animaVS.blocksRaycasts = false;
        CardManager.instance.ShowCard();
    }


    public void SetAvatarRemote(string url)
    {
        Debug.Log(url);
        UI_profileRemote.alpha = 1f;
        GetSpriteFromURL(url, (downloadedSprite) =>
        {
            // Callback ini akan berjalan setelah download selesai
            if (downloadedSprite != null)
            {
                Debug.Log("Sprite berhasil di-download! remote");
                if (avatarRemote != null)
                {
                    avatarRemote.sprite = downloadedSprite;
                    avatarRemoteUI.sprite = downloadedSprite;
                    GlobalVariable.avatarRemote = downloadedSprite;
                }
            }
            else
            {
                Debug.LogError("Gagal mendapatkan sprite dari URL.");
            }
        });
    }

    // =================== HTTP HELPERS ===================
    public void LoginWithCustomToken(string apiUrl, string customToken, Action<string> onSuccess, Action<string> onError = null)
    {
        Loading.instance.ShowLoading();
        string jsonBody = "{\"customToken\":\"" + customToken + "\"}";
        StartCoroutine(PostJsonRoutine(apiUrl, jsonBody, onSuccess, onError));
    }

    private IEnumerator PostJsonRoutine(string apiUrl, string jsonBody, Action<string> onSuccess, Action<string> onError)
    {
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        using (UnityWebRequest req = new UnityWebRequest(apiUrl, UnityWebRequest.kHttpVerbPOST))
        {
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
            if (req.result == UnityWebRequest.Result.Success)
#else
            if (!req.isNetworkError && !req.isHttpError)
#endif
            {
                onSuccess?.Invoke(req.downloadHandler.text);
                Loading.instance.HideLoading();
            }
            else
            {
                onError?.Invoke(req.error);
                Loading.instance.ShowErrorText(req.error);
            }
        }
    }

    public void PostJson(string apiUrl, string token, string jsonBody, Action<string> onSuccess, Action<string> onError = null, Action<string> msg = null)
    {
        Loading.instance.ShowLoading();
        StartCoroutine(PostJsonRoutineWithBearer(apiUrl, token, jsonBody, onSuccess, onError, msg));
    }

    private IEnumerator PostJsonRoutineWithBearer(string apiUrl, string token, string jsonBody, Action<string> onSuccess, Action<string> onError, Action<string> msg = null)
    {
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        using (UnityWebRequest req = new UnityWebRequest(apiUrl, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            if (!string.IsNullOrEmpty(token))
                req.SetRequestHeader("Authorization", "Bearer " + token);

            yield return req.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
            if (req.result == UnityWebRequest.Result.Success)
#else
            if (!req.isNetworkError && !req.isHttpError)
#endif
            {
                Loading.instance.HideLoading();
                onSuccess?.Invoke(req.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Error Response: " + req.downloadHandler.text);
                onError?.Invoke(req.error);
                msg?.Invoke(req.downloadHandler.text + req.downloadHandler.error);
                Loading.instance.ShowErrorText(req.error);
            }
        }
    }


    // ==== RTC Data Stream for Chat ====
    private int _dataStreamId = -1;
    public event Action<uint, string> OnChatMessage; // (uid, text)

    public int CreateRtcDataStream(bool reliable = true, bool ordered = true)
    {
        _dataStreamId = -1;
        int rc = _rtc.CreateDataStream(ref _dataStreamId, reliable, ordered);
        Debug.Log($"[RTC] CreateDataStream rc={rc}, id={_dataStreamId}");
        return _dataStreamId;
    }

    public bool SendChatText(string text)
    {
        if (!joinSuccess || _dataStreamId < 0) return false;
        if (string.IsNullOrEmpty(text)) return false;

        // RTC limit ~1KB per pesan
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(text);
        if (bytes.Length > 1024)
        {
            Debug.LogWarning($"[RTC] Chat too long ({bytes.Length} bytes). Split first.");
            return false;
        }
        int rc = _rtc.SendStreamMessage(_dataStreamId, bytes, (uint)bytes.Length);
        if (rc != 0) Debug.LogWarning($"[RTC] SendStreamMessage rc={rc}");
        return rc == 0;
    }


    //public void GetData_AllRoom_Available()
    //{

    //    GetDataRoutine(apiUrlAgoraGetAllRoom, data.data.accessToken,
    //        (json) =>
    //        {
    //            Debug.Log("Data agora available loaded: " + json);
    //            try
    //            {
    //                dataRoom = JsonUtility.FromJson<DataRoom>(json);
    //                CreateListRoom();

    //            }
    //            catch (System.Exception e)
    //            {
    //                Debug.LogError("Failed to parse agora room data: " + e.Message);

    //            }
    //        },
    //        (error) => {
    //            Debug.LogError("Failed to agora room data: " + error);
    //        }
    //    );
    //}

    public void GetDataRoutine(string apiUrl, string customToken, Action<string> onSuccess, Action<string> onError = null)
    {
        Loading.instance.ShowLoading();
        StartCoroutine(GetJsonRoutine(apiUrl, customToken, onSuccess, onError));
    }

    private IEnumerator GetJsonRoutine(string apiUrl, string token, Action<string> onSuccess, Action<string> onError)
    {
        using (UnityWebRequest req = UnityWebRequest.Get(apiUrl))
        {
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            if (!string.IsNullOrEmpty(token))
                req.SetRequestHeader("Authorization", "Bearer " + token);

            yield return req.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
            if (req.result == UnityWebRequest.Result.Success)

#else
        if (!req.isNetworkError && !req.isHttpError)
#endif
            {
                onSuccess?.Invoke(req.downloadHandler.text);
                Loading.instance.HideLoading();
            }
            else
            {
                onError?.Invoke(req.error);
                Loading.instance.ShowErrorText(req.error);
                if (onError != null)
                {
                    if (req.error.Contains("Unauthorized"))
                    {
                        PlayerPrefs.DeleteKey("dataJwt");
                        SceneManager.LoadSceneAsync("MainSceneAgora");

                    }
                }
            }
        }
    }

    public Transform posListRoom;
    public GameObject prefabRoom;
    public GameObject notifListEmty;

    //public void CreateListRoom()
    //{
    //    if (dataRoom == null || dataRoom.data == null || dataRoom.data.Count == 0)
    //    {
    //        Debug.Log("No room data available to create list.");
    //        return;
    //    }
    //    // Clear existing children
    //    foreach (Transform child in posListRoom)
    //    {
    //        Destroy(child.gameObject);
    //    }

    //    if(dataRoom.data.Count == 0)
    //    {
    //        notifListEmty.SetActive(true);
    //        return;
    //    }
    //    else
    //    {
    //        notifListEmty.SetActive(false);
    //    }

    //    // Create new room items
    //    foreach (var room in dataRoom.data)
    //    {
    //        GameObject roomItem = Instantiate(prefabRoom, posListRoom);
    //        RoomItem roomItemScript = roomItem.GetComponent<RoomItem>();
    //        Button button = roomItem.GetComponent<Button>();
    //        if (roomItemScript != null)
    //        {
    //            roomItemScript.SetDataRoom(room.id, room.roomId, room.roomName, room.title, room.host, room.maxParticipants, room.status);
    //            button.onClick.AddListener(() => {
    //                JoinRoomTokenChanel(roomItemScript.roomName);
    //                JoinBattle(room.id);
    //            });
    //        }
    //        else
    //        {
    //            Debug.LogWarning("RoomItem script not found on prefab.");
    //        }
    //    }
    //}

    public void JoinBattleAgora(string roomName, string battleID)
    {
        heartRateEstimator_Unified.StartVideo();
        JoinRoomTokenChanel(roomName);
        JoinBattle(battleID);
    }

    public string battleId;
    public bool isMaster = false;

    public void CretaeRoom(string room, string _battleId)
    {
        isMaster = true;
        _roomName = room;
        battleId = _battleId;


        string title = data.data.profile.username;
        int maxParticipants = 2;
        CreateRoomRequest createRoomRequest = new CreateRoomRequest(battleId, _roomName, _roomName, title, maxParticipants);
        string rawBody = JsonMapper.ToJson(createRoomRequest);

        string apiUrlAgoraGetAllRoom = GlobalVariable.baseUrlArenaGO + "/nestjsApi/api/snap-video-battles";

        PostJson(apiUrlAgoraGetAllRoom, data.data.accessToken, rawBody,
            (json) =>
            {
                Debug.Log("Room created successfully: " + json);
                JoinBattle(_battleId);
            },
            (err) => Debug.LogError("Failed to create room: " + err)


        );

        CreateRoomTokenChanel();
    }


    public void GetSpriteFromURL(string url, Action<Sprite> onComplete)
    {
        StartCoroutine(DownloadImage(url, onComplete));
    }

    private IEnumerator DownloadImage(string url, Action<Sprite> onComplete)
    {
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogWarning("URL is null or empty!");
            onComplete?.Invoke(null);
            yield break;
        }

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);

        // ====================================================================
        // Percobaan #1: Menambahkan DUA header agar lebih mirip browser
        //  request.SetRequestHeader("Content-Type", "application/json");
        // request.SetRequestHeader("Authorization", "Bearer " + token_);
        // ====================================================================

        // Opsi Debug: Cek apakah URL-nya berubah
        Debug.Log("Mengirim request ke URL: " + request.url);

        yield return request.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
        if (request.result == UnityWebRequest.Result.Success)
#else
    if (!request.isNetworkError && !request.isHttpError)
#endif
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            Sprite newSprite = Sprite.Create(
                texture,
                new Rect(0.0f, 0.0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100.0f
            );
            onComplete?.Invoke(newSprite);
        }
        else
        {
            Debug.LogError($"Failed to download image from {url}. Error: {request.error}");
            onComplete?.Invoke(null);
        }

        request.Dispose();
    }


    // =================== DTO BACKEND ===================
    [Serializable]
    public class GetTokenChanel
    {
        public string channelName;
        public string userAccount;
        public int expirationTimeInSeconds;
        public GetTokenChanel(string channel, string user, int expire)
        {
            channelName = channel;
            userAccount = user;
            expirationTimeInSeconds = expire;
        }
    }

    [Serializable]
    public class Data
    {
        public string token;
        public string channelName;
        public string userAccount;
        public int role;
        public string tokenType;
        public string appId;
        public int expireTime;
        public int issuedAt;
    }

    [Serializable]
    public class TokenResponseAGora
    {
        public bool success;
        public Data data;
        public string message;
    }



    /////////////////////////////////====================================/////////////////////////////
    ///
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    [Serializable]
    public class Datum
    {
        public string id;
        public string roomId;
        public string roomName;
        public string title;
        public string host;
        public List<string> participants;
        public int maxParticipants;
        public string status;
    }
    [Serializable]
    public class DataRoom
    {
        public List<Datum> data;
    }

    [Serializable]
    public class CreateRoomRequest
    {
        public string id;
        public string roomId;
        public string roomName;
        public string title;
        public List<string> participants;
        public int maxParticipants;

        public CreateRoomRequest(string id, string roomId, string roomName, string title, int maxParticipants)
        {
            this.id = id;
            this.roomId = roomId;
            this.roomName = roomName;
            this.title = title;
            this.participants = new List<string>();
            this.maxParticipants = maxParticipants;
        }
    }

    public class AddScore
    {
        public int score;
        public string status;

        public AddScore(int score, string status)
        {
            this.score = score;
            this.status = status;
        }
    }
}
