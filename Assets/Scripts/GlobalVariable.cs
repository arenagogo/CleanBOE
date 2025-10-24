using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//using UserDataKominfo;

public static class GlobalVariable
{
    public static GameObject avatarObject = null;
    public static string name = "";
    public static string token = "";
    public static string gender = "";
    public static string prefabname = "";
    // public static bool isMobileBrowser = false;
    public static int role;
    public static string boothCode = "";

    //  public static UserData dataUSer;
    public static string baseUrl = "";
    public static int walletCoin;
    public static string nickNameAgora = "";
    public static string nickNameAgoraRemote = "";
    public static uint heartRate = 0;

    // public static string baseUrlArenaGO = "https://asia-southeast2-arenago-dev.cloudfunctions.net";
    public static string baseUrlArenaGO = "https://asia-southeast1-arenago-a80c0.cloudfunctions.net";

    public static bool smartWatchConnected = false;

    public static float maxHeartRate = 100f;

    public static Sprite avatarLocal;
    public static Sprite avatarRemote;

    public static string email_local;

    public static GAMEMODE gamemode = GAMEMODE.FACEMODE;

    public static int BPM;
    public static bool onPlaying;

    public enum GAMEMODE
    {
        FACEMODE,
        SMARTWACTH,
        VOICEMODE
    }

    //public static List<GameObject> onlineFriendsObject;



    //public static bool UpdateReduceWalletCoin(int coin)
    //{
    //    if (GetDataFireBase.Instance.data.data.profile.walletCoins >= coin)
    //    {
    //        //Debug.Log("UpdateWalletCoin: Sufficient coins available.");
    //        GetDataFireBase.Instance.data.data.profile.walletCoins -= coin;
    //        GetDataFireBase.Instance.UpdateReduceWalletCoin(coin);
    //        walletCoin -= coin;
    //        DataDisplay.instance.SetDataDisplay();
    //        return true;
    //    }
    //    else
    //    {
    //        Debug.LogWarning("UpdateWalletCoin: Insufficient coins.");
    //        return false;
    //    }
    //}

    //public static void UpdateAddWalletCoin(int coin)
    //{
    //    //Debug.Log("UpdateWalletCoin: Sufficient coins available.");
    //    GetDataFireBase.Instance.data.data.profile.walletCoins += coin;
    //    GetDataFireBase.Instance.UpdateReduceWalletCoin(coin);
    //    walletCoin += coin;
    //    DataDisplay.instance.SetDataDisplay();
    //    Notification.Instance.ShowNotification($"You received {coin} coins!");
    //}

    public static Status STATUS = Status.STANDBY;
}
public enum Status
{
    STANDBY = 0,
    INVITING = 1,
    PLAYING = 2,
    INVITED = 3,
}
