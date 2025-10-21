using System;
using System.Collections;
using System.Collections.Generic;
using System.Text; // <-- TAMBAHKAN INI
using System.Threading.Tasks;
using Agora.Rtc.LitJson;
using Agora.Rtm;
using MoodMe;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RtmChannelManager : MonoBehaviour
{
    public static RtmChannelManager instant;

    public FriendListsManager friendListsManager;

    public SimpleAgoraController_Unified controler;
    public HeartRateEstimator_Unified heartRateEstimator;
    public class UserIDRtm
    {
        public string userId;
        public int expirationTimeInSeconds;
        public UserIDRtm(string _userId)
        {
            this.userId = _userId;
            expirationTimeInSeconds = 7200;
        }
    }

    [System.Serializable]
    public class Data
    {
        public string token;
    }

    [System.Serializable]
    public class TokenRTM
    {
        public Data data;
    }

    private const string channelName = "ChanelBattleOfEmotiom";
    private string apiGenTokenRtm = GlobalVariable.baseUrlArenaGO + "/nestjsApi/api/agora/rtm-token-user";

    private IRtmClient rtmClient;

    public List<string> onlineUser = new List<string>();

    private void Awake()
    {
        instant = this;
    }

    private void Start()
    {
        StartCoroutine(CheckUserOnline());
    }

    IEnumerator CheckUserOnline()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            PrintOnlineUsers();
        }
    }



    public void SetChanelRTM(string _username)
    {
        Debug.Log($"[RTM] SetChanelRTM dipanggil untuk user: {_username}");

        UserIDRtm userID = new UserIDRtm(_username);
        string rawBody = JsonMapper.ToJson(userID);
        Debug.Log($"[RTM] Request Token Body: {rawBody}");

        controler.PostJson(apiGenTokenRtm, controler.data.data.accessToken, rawBody,
            (json) =>
            {
                Debug.Log($"[RTM] Token Response: {json}");
                TokenRTM tokenRTM = JsonUtility.FromJson<TokenRTM>(json);
                LoginChanelBattleOfEmotiom(_username, tokenRTM.data.token);
            },
            (err) => Debug.LogError("[RTM] Gagal minta token rtm: " + err)
        );
    }

    [SerializeField] private string app_ID = "";

    public async void LoginChanelBattleOfEmotiom(string _username, string _tokenRtm)
    {
        Debug.Log($"[RTM] Mulai login dengan user: {_username}, token: {_tokenRtm}");

        var config = new RtmConfig
        {
            appId = app_ID,
            userId = _username
        };

        rtmClient = RtmClient.CreateAgoraRtmClient(config.appId, config);

        Debug.Log("[RTM] Pasang event handler");
        rtmClient.OnPresenceEvent += OnPresenceEvent;
        rtmClient.OnMessageEvent += OnMessageEvent;

        rtmClient.OnConnectionStateChanged += (channel, state, reason) =>
        {
            Debug.Log($"[RTM] ConnectionStateChanged => state: {state}, reason: {reason}");
        };

        var loginResult = await rtmClient.LoginAsync(_tokenRtm);
        Debug.Log($"[RTM] LoginAsync result: Error={loginResult.Status.Error}, Reason={loginResult.Status.Reason}");
        if (loginResult.Status.Error)
        {
            Debug.LogError($"[RTM] Gagal Login: {loginResult.Status.Reason}");
            return;
        }
        Debug.Log($"[RTM] Berhasil Login sebagai: {_username}");

        var options = new SubscribeOptions { withPresence = true, withMessage = true };
        var subscribeResult = await rtmClient.SubscribeAsync(channelName, options);
        Debug.Log($"[RTM] SubscribeAsync result: Error={subscribeResult.Status.Error}, Reason={subscribeResult.Status.Reason}");
        if (subscribeResult.Status.Error)
        {
            Debug.LogError($"[RTM] Gagal Subscribe ke Channel {channelName}: {subscribeResult.Status.Reason}");
            return;
        }
        Debug.Log($"[RTM] Berhasil Subscribe ke Channel: {channelName}");

        await GetCurrentMembers();
    }

    public RtmResult<WhoNowResult> resultWhoNow;

    private async Task GetCurrentMembers()
    {
        Debug.Log("[RTM] Panggil GetCurrentMembers()");
        var presenceOptions = new PresenceOptions();
        var result = await rtmClient.GetPresence().WhoNowAsync(channelName, RTM_CHANNEL_TYPE.MESSAGE, presenceOptions);
        resultWhoNow = result;

        Debug.Log($"[RTM] WhoNowAsync result: Error={result.Status.Error}, Reason={result.Status.Reason}");
        if (result.Status.Error)
        {
            Debug.LogError($"[RTM] Gagal mendapatkan daftar member: {result.Status.Reason}");
            return;
        }

        onlineUser.Clear();
        foreach (var member in result.Response.UserStateList)
        {
            onlineUser.Add(member.userId);
            Debug.Log($"[RTM] Member ditemukan: {member.userId}");
        }
    }

    public void PrintOnlineUsers()
    {
        Debug.Log("[RTM] PrintOnlineUsers()");
        foreach (var member in onlineUser)
        {
            Debug.Log($"[RTM] OnlineUser: {member}");
            OnlineUsers(member);
        }
    }

    private void OnPresenceEvent(PresenceEvent e)
    {
        Debug.Log($"[RTM] PresenceEvent: {e.type}, publisher={e.publisher}");

        if (e.type == RTM_PRESENCE_EVENT_TYPE.REMOTE_JOIN)
        {
            Debug.Log($"[RTM] Pengguna Bergabung: {e.publisher}");
            if (!onlineUser.Contains(e.publisher))
            {
                onlineUser.Add(e.publisher);
            }
            OnlineUsers(e.publisher);
        }
        else if (e.type == RTM_PRESENCE_EVENT_TYPE.REMOTE_LEAVE)
        {
            Debug.Log($"[RTM] Pengguna Keluar: {e.publisher}");
            onlineUser.Remove(e.publisher);
            OfflineUsers(e.publisher);
        }
    }

    private void OfflineUsers(string offlineUser)
    {
        Debug.Log($"[RTM] OfflineUsers: {offlineUser}");
        foreach (GameObject g in friendListsManager.friendsDataPrefabs)
        {
            if (g.name == offlineUser)
            {
                g.GetComponent<FriendListDataPrefab>().SetFriendlist_OFFLINE();
            }
        }
    }

    private void OnlineUsers(string OnlineUser)
    {
        Debug.Log($"[RTM] OnlineUsers: {OnlineUser}");
        foreach (GameObject g in friendListsManager.friendsDataPrefabs)
        {
            if (g.name == OnlineUser)
            {
                g.GetComponent<FriendListDataPrefab>().SetFriendlist_ONLINE();
            }
        }
    }

    public string nameFirendInvite = "";

    //=================INVITE========
    public async void InviteFriend(string friendUsername, string myname, string avatarUrl, string gamemode)
    {
        //  Debug.Log($"[RTM] InviteFriend dipanggil: target={friendUsername}, myname={myname}");
        nameFirendInvite = friendUsername;
        bool success = await SendInviteCommandAsync("invite", friendUsername, myname, avatarUrl, gamemode);

        if (success)
        {
            Debug.Log($"[RTM] Undangan berhasil dikirim! {gamemode}");
        }
        else
        {
            Debug.LogError("[RTM] Gagal mengirim undangan.");
        }
    }

    public async void AcceptInvite(string roomName, string nameAccept, string battleId)
    {
        Debug.Log($"[RTM] InviteFriend dipanggil: target={roomName}, myname={nameAccept}");

        bool success = await SendInviteCommandAsync("AcceptInvite", roomName, nameAccept, battleId, "");

        if (success)
        {
            Debug.Log("[RTM] AcceptInvite berhasil dikirim!");
        }
        else
        {
            Debug.LogError("[RTM] Gagal mengirim AcceptInvite.");
        }
    }

    public async void RejectInvite(string roomName, string nameAccept, string battleId)
    {
        Debug.Log($"[RTM] InviteFriend dipanggil: target={roomName}, myname={nameAccept}");

        bool success = await SendInviteCommandAsync("RejectInvite", roomName, nameAccept, battleId, "");

        if (success)
        {
            Debug.Log("[RTM] RejectInvite berhasil dikirim!");
        }
        else
        {
            Debug.LogError("[RTM] Gagal mengirim RejectInvite.");
        }
    }

    public async void SendScore(string score, string nameAccept, string battleId)
    {
        bool success = await SendInviteCommandAsync("SendScore", score, nameAccept, battleId, "");

        if (success)
        {
            Debug.Log("[RTM] SendScore berhasil dikirim!");
        }
        else
        {
            Debug.LogError("[RTM] Gagal mengirim SendScore");
        }
    }

    public async void SenDevice(string _deviceInfo, string battleId)
    {
        bool success = await SendInviteCommandAsync("DeviceInfo", _deviceInfo, _deviceInfo, battleId, "");

        if (success)
        {
            Debug.Log("[RTM] SendScore berhasil dikirim!");
        }
        else
        {
            Debug.LogError("[RTM] Gagal mengirim SendScore");
        }
    }

    public void GoSendTrackFace(string jsonDataFace)
    {
        SendTrackFace(jsonDataFace, controler.battleId);
    }

    async void SendTrackFace(string jsonDataFace, string battleId)
    {
        bool success = await SendInviteCommandAsync("FaceTracking", jsonDataFace, jsonDataFace, battleId, "");

        if (success)
        {
            Debug.Log("[RTM] FaceTracking berhasil dikirim!");
        }
        else
        {
            Debug.LogError("[RTM] Gagal mengirim FaceTracking");
        }
    }

    public void GoScanFaceRemote()
    {
        ScanFaceRemote(controler.battleId);
    }

    async void ScanFaceRemote(string battleId)
    {
        bool success = await SendInviteCommandAsync("ScanFaceRemote", battleId, battleId, battleId, "");

        if (success)
        {
            Debug.Log("[RTM] ScanFaceRemote berhasil dikirim!");
        }
        else
        {
            Debug.LogError("[RTM] Gagal mengirim SScanFaceRemote");
        }
    }

    public async Task<bool> SendInviteCommandAsync(string typeMessage, string recipientId, string myname, string avatarUrl, string gamemode)
    {
        // Debug.Log($"[RTM] SendInviteCommandAsync dipanggil, target={recipientId}, myname={myname}");

        if (rtmClient == null)
        {
            Debug.LogError($"[RTM] RTM Client belum diinisialisasi. Gagal mengirim {typeMessage}");
            return false;
        }

        string localUserId = controler.data.data.profile.username;
        // Debug.Log($"[RTM] LocalUserId = {localUserId}");

        RtmCommand command = new RtmCommand
        {
            commandType = typeMessage,
            senderId = localUserId,
            recipientId = recipientId,
            payload = avatarUrl,
            gamemode = gamemode,
        };

        string messageString = JsonMapper.ToJson(command);
        //  Debug.Log($"[RTM] Payload JSON: {messageString}");

        var options = new PublishOptions { channelType = RTM_CHANNEL_TYPE.MESSAGE };
        var result = await rtmClient.PublishAsync(channelName, messageString, options);

        //  Debug.Log($"[RTM] PublishAsync result: Error={result.Status.Error}, Reason={result.Status.Reason}");
        if (result.Status.Error)
        {
            Debug.LogError($"[RTM] Gagal mengirim invite ke {recipientId}: {result.Status.Reason}");
            return false;
        }

        Debug.Log($"[RTM] Berhasil mengirim invite ke {recipientId}");
        // friendListsManager.IncomingInvite(myname, recipientId);
        return true;
    }


    private void OnMessageEvent(MessageEvent e)
    {
        // Debug.Log($"[RTM] CALLBACK OnMessageEvent terpanggil dari publisher={e.publisher}");

        string messageJson = e.message.GetData<string>();
        // Debug.Log($"[RTM] Isi Pesan (raw): {messageJson}");

        try
        {
            RtmCommand receivedCommand = JsonMapper.ToObject<RtmCommand>(messageJson);
            // Debug.Log($"[RTM] CommandType={receivedCommand.commandType}, Sender={receivedCommand.senderId}, Recipient={receivedCommand.recipientId}");


            switch (receivedCommand.commandType)
            {
                case "invite":
                    if (receivedCommand.recipientId == controler.data.data.profile.username)
                    {
                        Debug.Log($"incoming {receivedCommand.gamemode}");
                        friendListsManager.IncomingInvite(receivedCommand.senderId, receivedCommand.recipientId, receivedCommand.payload, receivedCommand.gamemode);

                    }
                    break;

                case "AcceptInvite":

                    Debug.Log($"mencoba masuk room {receivedCommand.senderId} {receivedCommand.recipientId} {receivedCommand.payload}");

                    if (nameFirendInvite == receivedCommand.senderId)
                    {
                        controler.battleId = receivedCommand.payload;
                        controler.JoinBattleAgora(receivedCommand.recipientId, receivedCommand.payload);
                        Debug.Log($"mencoba masuk room {receivedCommand.recipientId} {receivedCommand.payload} {receivedCommand.payload}");
                    }
                    break;

                case "RejectInvite":

                    Debug.Log($"mencoba RejectInvite room {receivedCommand.senderId} {receivedCommand.recipientId} {receivedCommand.payload}");

                    if (nameFirendInvite == receivedCommand.senderId)
                    {
                        controler.battleId = "";
                        GlobalVariable.STATUS = Status.STANDBY;
                        REJECTINVITE.Instance.SetRejectOpen(receivedCommand.senderId);
                        Debug.Log($"{receivedCommand.senderId} menolak invite dari anda");
                    }


                    break;


                case "SendScore":

                    Debug.Log($"mencoba kirim SendScore  {receivedCommand.senderId} {receivedCommand.recipientId} {receivedCommand.payload}");
                    GameScoreManager.instance.UpdateScoreRemote(receivedCommand.senderId, receivedCommand.recipientId, receivedCommand.payload);
                    break;

                case "DeviceInfo":

                    Debug.Log($"mencoba kirim deviceinfo  {receivedCommand.senderId} {receivedCommand.recipientId} {receivedCommand.payload}");
                    if (controler.battleId == receivedCommand.payload && controler.battleId != "")
                    {
                        deviceRemotetext.text = receivedCommand.recipientId;
                    }

                    break;

                case "FaceTracking":

                    Debug.Log($"mencoba FaceTracking  {receivedCommand.senderId} {receivedCommand.recipientId} {receivedCommand.payload}");
                    if (controler.battleId == receivedCommand.payload && controler.battleId != "")
                    {
                        AnimScore.Instance.ReceiverDataScore(receivedCommand.recipientId);
                    }
                    break;
                case "ScanFaceRemote":
                    Debug.Log($"[RTM] Menerima ScanFaceRemote dari {receivedCommand.senderId}");
                    if (controler.battleId == receivedCommand.payload && controler.battleId != "")
                    {
                        Debug.Log("[RTM] Menjadwalkan GoScanFace() di main thread...");
                        MainThreadDispatcher.RunOnMainThread(() =>
                   {
                       GoScanFace();
                   });
                    }
                    break;


            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[RTM] Gagal parse JSON: {ex.Message}, Raw={messageJson}");
        }
    }

    void GoScanFace()
    {
        Debug.Log("Memanggil ScanFace di ManageEmotionsNetwork");
        StartCoroutine(ManageEmotionsNetwork.instant.GetValue());
    }

    public void DeviceInfoLocal(string deviceInfo)
    {
        deviceLocaltext.text = deviceInfo;
    }

    public TextMeshProUGUI deviceLocaltext;
    public TextMeshProUGUI deviceRemotetext;

    private async void OnApplicationQuit()
    {
        if (rtmClient != null)
        {
            try
            {
                await rtmClient.UnsubscribeAsync(channelName);
                await rtmClient.LogoutAsync();
                rtmClient.Dispose();
                heartRateEstimator.StopVideo();
                Debug.Log("[RTM] Client sudah dibersihkan (logout, unsubscribe, dispose).");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[RTM] Error saat cleanup: {ex.Message}");
            }
        }
    }

    public async void LeaveRtmChannel()
    {
        if (rtmClient != null)
        {
            try
            {
                await rtmClient.UnsubscribeAsync(channelName);
                await rtmClient.LogoutAsync();
                rtmClient.Dispose();
                heartRateEstimator.StopVideo();
                SceneManager.LoadScene("MainSceneAgora");
                Debug.Log("[RTM] Client sudah dibersihkan (logout, unsubscribe, dispose).");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[RTM] Error saat cleanup: {ex.Message}");
            }
        }
    }

    public async void LeaveRtmChannelSurender()
    {
        if (rtmClient != null)
        {
            try
            {
                controler.FoceQuit();
                await rtmClient.UnsubscribeAsync(channelName);
                await rtmClient.LogoutAsync();
                controler.battleId = "";
                rtmClient.Dispose();
                heartRateEstimator.StopVideo();
                SceneManager.LoadScene("MainSceneAgora");
                Debug.Log("[RTM] Client sudah dibersihkan (logout, unsubscribe, dispose).");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[RTM] Error saat cleanup: {ex.Message}");
            }
        }
    }

    public async void LogoutRtm()
    {
        if (rtmClient != null)
        {
            try
            {
                await rtmClient.UnsubscribeAsync(channelName);
                await rtmClient.LogoutAsync();
                rtmClient.Dispose();
                heartRateEstimator.StopVideo();
                PlayerPrefs.DeleteKey("dataJwt");
                SceneManager.LoadScene("MainSceneAgora");
                Debug.Log("[RTM] Client sudah dibersihkan (logout, unsubscribe, dispose).");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[RTM] Error saat cleanup: {ex.Message}");
            }
        }
    }

    async void ForceQuit()
    {
        if (rtmClient != null)
        {
            try
            {
                await rtmClient.UnsubscribeAsync(channelName);
                await rtmClient.LogoutAsync();
                rtmClient.Dispose();
                heartRateEstimator.StopVideo();
                SceneManager.LoadScene("MainSceneAgora");
                Debug.Log("[RTM] Client sudah dibersihkan (logout, unsubscribe, dispose).");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[RTM] Error saat cleanup: {ex.Message}");
            }
        }
    }

    private bool onBackground = false;

    private void OnApplicationPause(bool pauseStatus)
    {
        //if (pauseStatus)
        //{
        //    ForceQuit();
        //    controler.FoceQuit();
        //    onBackground = true;
        //}
        //else
        //{
        //    if(onBackground)
        //    {
        //        SceneManager.LoadScene("MainSceneAgora");
        //    }

        //}
    }

    [Serializable]
    public class RtmCommand
    {
        public string commandType;
        public string senderId;
        public string recipientId;
        public string payload;
        public string gamemode;
    }
}
