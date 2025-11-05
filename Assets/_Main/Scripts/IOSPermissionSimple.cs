using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IOSPermissionSimple : MonoBehaviour
{
    public static IOSPermissionSimple instance;

    public WebcamToRenderTexture webcamToRenderTexture;
    public Button requestButton;
    public TextMeshProUGUI statusText;
    public CanvasGroup cvsPermission;

    private bool alreadyRequested;

    private void Awake()
    {
        instance = this;

        // Cek apakah sebelumnya sudah pernah meminta izin
        alreadyRequested = PlayerPrefs.HasKey("status_permission_key");

        // Jika izin sudah diberikan → sembunyikan UI
        if (CheckPermissionGranted())
        {
            HidePermissionUI();
        }
        else
        {
            // Jika belum pernah diminta → tampilkan pesan awal
            // Jika sudah pernah ditolak → tampilkan pesan penolakan
            ShowPermissionUI(alreadyRequested
                ? "Camera and microphone access are required to analyze your emotions during gameplay. Access is currently disabled."
                : "Emotion Duel uses your camera and microphone to analyze your emotions in real time. Your data is used only during gameplay and is never stored.");
        }
    }

    void Start()
    {
        requestButton.onClick.AddListener(OnRequestPermission);
    }

    void OnRequestPermission()
    {
        if (CheckPermissionGranted())
        {
            HidePermissionUI();
            // OpenCame();
            return;
        }

#if UNITY_IOS && !UNITY_EDITOR
        StartCoroutine(RequestPermissionsIOS());
#elif UNITY_ANDROID && !UNITY_EDITOR
        StartCoroutine(RequestPermissionsAndroid());
#else
        Debug.Log("In Editor: Simulating permission granted.");
        HidePermissionUI();
        // OpenCame();
#endif
    }

#if UNITY_IOS && !UNITY_EDITOR
    private IEnumerator RequestPermissionsIOS()
    {
        // Munculkan popup sistem iOS (first time)
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
            yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);

        if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
            yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);

        // Simpan bahwa sudah pernah request
        PlayerPrefs.SetInt("status_permission_key", 1);
        PlayerPrefs.Save();
        alreadyRequested = true;

        // Cek hasil
        if (CheckPermissionGranted())
        {
            HidePermissionUI();
           // OpenCame();
        }
        else
        {
            // Izin ditolak → Button disembunyikan
            ShowPermissionUI("Camera and microphone access are required to use this feature.\nYou can continue without it.");
            requestButton.gameObject.SetActive(false);
        }
    }
#endif


#if UNITY_ANDROID && !UNITY_EDITOR
    private IEnumerator RequestPermissionsAndroid()
    {
        UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Camera);
        UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Microphone);

        yield return new WaitForSeconds(0.5f);

        if (CheckPermissionGranted())
        {
            HidePermissionUI();
           // OpenCame();
        }
        else
        {
            ShowPermissionUI("Camera and microphone access are required.\nAccess was denied.");
            requestButton.gameObject.SetActive(false);
        }
    }
#endif


    public bool CheckPermissionGranted()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera) &&
               UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone);
#elif UNITY_IOS && !UNITY_EDITOR
        return Application.HasUserAuthorization(UserAuthorization.WebCam) &&
               Application.HasUserAuthorization(UserAuthorization.Microphone);
#else
        return true;
#endif
    }


    private void HidePermissionUI()
    {
        cvsPermission.alpha = 0;
        cvsPermission.interactable = false;
        cvsPermission.blocksRaycasts = false;
        requestButton.gameObject.SetActive(false);
    }

    private void ShowPermissionUI(string msg)
    {
        cvsPermission.alpha = 1;
        cvsPermission.interactable = true;
        cvsPermission.blocksRaycasts = true;
        statusText.text = msg;
        requestButton.gameObject.SetActive(true);
    }


    public void OpenCame()
    {
        if (webcamToRenderTexture != null)
        {
            Debug.Log("🎥 Starting webcam after permission granted...");
            webcamToRenderTexture.StartWebcam();
        }
        else
        {
            Debug.LogWarning("⚠ WebcamToRenderTexture reference not assigned!");
        }
    }
}
