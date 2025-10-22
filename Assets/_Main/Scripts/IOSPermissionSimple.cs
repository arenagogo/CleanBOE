using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_IOS
using UnityEngine.iOS;
#endif

public class IOSPermissionSimple : MonoBehaviour
{
    public WebcamToRenderTexture webcamToRenderTexture;
    public Button requestButton;
    public TextMeshProUGUI statusText;
    public CanvasGroup cvsPermission;

    private bool alreadyRequested;

    private void Awake()
    {
        // Cek kalau sudah pernah diminta
        alreadyRequested = PlayerPrefs.HasKey("status_permission_key");

        if (CheckPermissionGranted())
        {
            // Izin sudah diberikan → langsung sembunyikan UI & buka kamera
            HidePermissionUI();
            OpenCame();
        }
        else
        {
            ShowPermissionUI(alreadyRequested
                ? "Camera/Mic access denied. Please enable in Settings."
                : "This game requires permission to use the camera and microphone.");
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
            OpenCame();
            return;
        }

#if UNITY_IOS && !UNITY_EDITOR
        if (alreadyRequested)
        {
            Application.OpenURL("app-settings:");
        }
        else
        {
            StartCoroutine(RequestPermissionsIOS());
        }
#elif UNITY_ANDROID && !UNITY_EDITOR
        if (alreadyRequested &&
            (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera) ||
             !UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone)))
        {
            Application.OpenURL("package:" + Application.identifier);
        }
        else
        {
            StartCoroutine(RequestPermissionsAndroid());
        }
#else
        Debug.Log("In Editor: Simulating permission granted.");
        HidePermissionUI();
        OpenCame();
#endif
    }

#if UNITY_IOS && !UNITY_EDITOR
    private IEnumerator RequestPermissionsIOS()
    {
        // Minta izin kamera
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        }

        // Minta izin mic
        if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
        {
            yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
        }

        // Tandai sudah diminta
        PlayerPrefs.SetInt("status_permission_key", 1);
        PlayerPrefs.Save();
        alreadyRequested = true;

        // Cek ulang hasilnya
        if (CheckPermissionGranted())
        {
            HidePermissionUI();
            OpenCame(); // ← otomatis buka kamera
        }
        else
        {
            ShowPermissionUI("Camera/Mic access denied. Please enable in Settings.");
        }
    }
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
    private IEnumerator RequestPermissionsAndroid()
    {
        UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Camera);
        UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Microphone);

        // Tunggu beberapa saat untuk respon user
        yield return new WaitForSeconds(1.2f);

        PlayerPrefs.SetInt("status_permission_key", 1);
        PlayerPrefs.Save();
        alreadyRequested = true;

        if (CheckPermissionGranted())
        {
            HidePermissionUI();
            OpenCame(); // ← otomatis buka kamera
        }
        else
        {
            ShowPermissionUI("Camera/Mic access denied. Please enable in Settings.");
        }
    }
#endif

    private bool CheckPermissionGranted()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera) &&
               UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone);
#elif UNITY_IOS && !UNITY_EDITOR
        return Application.HasUserAuthorization(UserAuthorization.WebCam) &&
               Application.HasUserAuthorization(UserAuthorization.Microphone);
#else
        // Di Editor selalu true
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

    private void OpenCame()
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
