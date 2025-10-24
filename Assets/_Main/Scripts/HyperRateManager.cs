using System;
using System.Collections;
using System.Collections.Generic;
using NativeWebSocket;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class HyperRateManager : MonoBehaviour
{
    public static HyperRateManager Instance;
    public MainMenuSelecting mainMenuSelecting;
    // Put your websocket Token ID here
    public string websocketToken = "<Request your Websocket Token>"; //You don't have one, get it here https://www.hyperate.io/api
                                                                     // public string hyperateID = "internal-testing";
                                                                     // Textbox to display your heart rate i
                                                                     // Websocket for connection with Hyperate
    WebSocket websocket;

    private string hyperRateValue = "0";
    [SerializeField] private string bpm;

    [SerializeField] private bool onConected = false;
    [SerializeField] private bool onConectedDevice = false;

    [SerializeField] private float timeToCheckConnection = 0f;

    public GameObject notif;
    public GameObject frameFormSmartWacth;
    public GameObject frameFaceRecontion;
    public GameObject DropdownSelectDevice;
    private string keyIdKey = "HYPERATE_ID";

    public static event Action<string> OnBPMChanged;
    private string _bpm;

    public GameObject buttonDisconnet;

    public GameObject iconSmatrWatchConenected;
    public SetupGameMode setupGameMode;

    public string BPM
    {
        get => _bpm;
        private set
        {
            _bpm = value;
            // Panggil event jika ada yang subscribe
            OnBPMChanged?.Invoke(_bpm);
        }
    }

    private void Start()
    {
        if (PlayerPrefs.HasKey(keyIdKey))
        {
            ConnectingHyperRate();
            mainMenuSelecting.inputIdSmartWach.text = PlayerPrefs.GetString(keyIdKey);
        }

    }

    public async void ConnectingHyperRate()
    {
        mainMenuSelecting.buttonConnect.interactable = false;
        mainMenuSelecting.statusDevice.color = Color.red;
        websocket = new WebSocket("wss://app.hyperate.io/socket/websocket?token=" + websocketToken);
        Debug.Log("Connect!");
        mainMenuSelecting.statusDevice.text = "Connecting";
        onConected = true;
        onConectedDevice = true;

        websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
            mainMenuSelecting.statusDevice.text = "Connection open!";
            mainMenuSelecting.statusDevice.color = Color.red;
            SendWebSocketMessage();
        };

        websocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
            mainMenuSelecting.statusDevice.text = "Error! " + e;
            mainMenuSelecting.statusDevice.color = Color.red;
            GlobalVariable.smartWatchConnected = false;
            iconSmatrWatchConenected.SetActive(false);
            //SetupGameMode.instance.SetGameModeSmartWatch(false);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
            mainMenuSelecting.statusDevice.text = "Connection closed!";
            mainMenuSelecting.statusDevice.color = Color.red;
            mainMenuSelecting.buttonConnect.interactable = true;
            // PlayerPrefs.DeleteKey(keyIdKey);
            mainMenuSelecting.inputIdSmartWach.text = "";
            GlobalVariable.smartWatchConnected = false;
            iconSmatrWatchConenected.SetActive(false);
            // SetupGameMode.instance.SetGameModeSmartWatch(false);
        };

        websocket.OnMessage += (bytes) =>
        {
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            var msg = JObject.Parse(message);

            if (msg["event"].ToString() == "hr_update")
            {
                hyperRateValue = (string)msg["payload"]["hr"];
                bpm = HyperRateValue();
                BPM = bpm;
                mainMenuSelecting.statusDevice.text = "Device Connected";
                mainMenuSelecting.statusDevice.color = Color.green;
                timeToCheckConnection = 0f;
                // GlobalVariable.gamemode = GlobalVariable.GAMEMODE.SMARTWACTH;
                GlobalVariable.smartWatchConnected = true;
                buttonDisconnet.SetActive(true);
                mainMenuSelecting.buttonConnect.gameObject.SetActive(false);
                iconSmatrWatchConenected.SetActive(true);
                // setupGameMode.SetGameModeSmartWatch();
                // setupGameMode.toggleFaceMode.isOn = false;
                // setupGameMode.toggleSmartWatch.isOn = true;
                // mainMenuSelecting.inputIdSmartWach.text = "";
                // SetupGameMode.instance.SetGameModeSmartWatch(true);
            }
        };

        // Heartbeat
        InvokeRepeating("SendHeartbeat", 1.0f, 25.0f);

        // Mulai timeout disconnect (10 detik)
        StartCoroutine(ConnectionTimeout(60f));

        await websocket.Connect();
    }

    private IEnumerator ConnectionTimeout(float seconds)
    {
        float timer = 0f;
        GlobalVariable.gamemode = GlobalVariable.GAMEMODE.FACEMODE;

        while (timer < seconds)
        {
            if (GlobalVariable.gamemode == GlobalVariable.GAMEMODE.SMARTWACTH) yield break; // sudah connect, batal timeout
            timer += Time.deltaTime;
            yield return null;
        }

        // Timeout: disconnect otomatis
        Debug.LogWarning("[HyperRate] Connection timeout. Auto disconnect.");
        // DisconnectHyperRate();
    }

    [ContextMenu("KONEK SMARTWATCH")]
    public void KonekSmartwatch()
    {
        GlobalVariable.smartWatchConnected = true;
        iconSmatrWatchConenected.SetActive(true);
        // SetupGameMode.instance.SetGameModeSmartWatch(true);
    }

    [ContextMenu("DISKONEK SMARTWATCH")]
    public void DiskonekSmartwatch()
    {
        GlobalVariable.smartWatchConnected = false;
        iconSmatrWatchConenected.SetActive(false);
        // SetupGameMode.instance.SetGameModeSmartWatch(false);
    }


    private void Awake()
    {
        Instance = this;
    }

    public string HyperRateValue()
    {
        return hyperRateValue;
    }

    void Update()
    {
        if (onConectedDevice)
        {
            timeToCheckConnection += Time.deltaTime;
            if (timeToCheckConnection >= 10f)
            {
                timeToCheckConnection = 0f;
                onConectedDevice = false;
                mainMenuSelecting.statusDevice.text = "No Device Conected";
                mainMenuSelecting.statusDevice.color = Color.red;
                _ = websocket.Close(); // Fix: Use discard to explicitly ignore the returned Task
                notif.SetActive(true);
            }
        }

#if !UNITY_WEBGL || UNITY_EDITOR
        if (onConected)
            websocket.DispatchMessageQueue();
#endif
    }

    async void SendWebSocketMessage()
    {
        if (websocket.State == WebSocketState.Open)
        {

            if (PlayerPrefs.HasKey(keyIdKey))
            {
                string hrid = PlayerPrefs.GetString(keyIdKey);
                await websocket.SendText("{\"topic\": \"hr:" + hrid + "\", \"event\": \"phx_join\", \"payload\": {}, \"ref\": 0}");
            }
            else
            {
                await websocket.SendText("{\"topic\": \"hr:" + mainMenuSelecting.hypeRateID + "\", \"event\": \"phx_join\", \"payload\": {}, \"ref\": 0}");
                PlayerPrefs.SetString(keyIdKey, mainMenuSelecting.hypeRateID);
            }
        }
    }
    async void SendHeartbeat()
    {
        if (websocket.State == WebSocketState.Open)
        {
            // Send heartbeat message in order to not be suspended from the connection
            await websocket.SendText("{\"topic\": \"phoenix\",\"event\": \"heartbeat\",\"payload\": {},\"ref\": 0}");

        }
    }

    private async void OnApplicationQuit()
    {
        if (websocket != null)
        {
            try
            {
                await websocket.Close();
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Failed to close websocket: " + ex.Message);
            }
        }
    }

    public async void DisconnectHyperRate()
    {
        if (websocket != null)
        {
            try
            {
                Debug.Log("[HyperRate] Disconnecting...");
                await websocket.Close();

                onConected = false;
                onConectedDevice = false;
                GlobalVariable.gamemode = GlobalVariable.GAMEMODE.FACEMODE;
                mainMenuSelecting.inputIdSmartWach.text = "";

                if (mainMenuSelecting != null)
                    mainMenuSelecting.statusDevice.text = "Disconnected";

                PlayerPrefs.DeleteKey(keyIdKey);
                mainMenuSelecting.inputIdSmartWach.text = "";
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[HyperRate] Failed to disconnect: " + ex.Message);
            }
        }
        else
        {
            Debug.Log("[HyperRate] No active websocket to disconnect.");
        }
        GlobalVariable.smartWatchConnected = false;
        iconSmatrWatchConenected.SetActive(false);
    }


}



public class HyperateResponse
{
    public string Event { get; set; }
    public string Payload { get; set; }
    public string Ref { get; set; }
    public string Topic { get; set; }
}
