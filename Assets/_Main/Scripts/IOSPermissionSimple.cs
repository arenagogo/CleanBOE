using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_IOS
using UnityEngine.iOS;
#endif

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

        alreadyRequested = PlayerPrefs.HasKey("status_permission_key");

        if (CheckPermissionGranted())
        {
            HidePermissionUI();
        }
        else
        {
            ShowPermissionUI(alreadyRequested
                ? "Camera and microphone access are required to analyze your emotions during gameplay. Please enable access in Settings to continue."
                : "Emotion Duel uses your camera and microphone to analyze your emotions in real time. Your data is used only during gameplay and is never stored.");
        }
    }

    public void OpenCamera()
    {
        alreadyRequested = PlayerPrefs.HasKey("status_permission_key");

        if (CheckPermissionGranted())
        {
            HidePermissionUI();
            OpenCame();
        }
        else
        {
            ShowPermissionUI(alreadyRequested
                ? "Camera and microphone access are required to analyze your emotions during gameplay. Please enable access in Settings to continue."
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
            OpenCame();
            return;
        }

#if UNITY_IOS && !UNITY_EDITOR
        if (alreadyRequested)
        {
            // Langsung buka setting iOS
            Application.OpenURL("app-settings:");
        }
        else
        {
            StartCoroutine(RequestPermissionsIOS());
        }

#elif UNITY_ANDROID && !UNITY_EDITOR
        StartCoroutine(RequestPermissionsAndroid());
#else
        Debug.Log("In Editor: Simulating permission granted.");
        HidePermissionUI();
        OpenCame();
#endif
    }

#if UNITY_IOS && !UNITY_EDITOR
    private IEnumerator RequestPermissionsIOS()
    {
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
            yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);

        if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
            yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);

        PlayerPrefs.SetInt("status_permission_key", 1);
        PlayerPrefs.Save();
        alreadyRequested = true;

        if (CheckPermissionGranted())
        {
            HidePermissionUI();
            OpenCame();
        }
        else
        {
            ShowPermissionUI("This feature uses your camera and microphone to detect your emotions during gameplay. Access is currently disabled — you can enable it anytime from Settings to experience full features.");
            // Ubah fungsi tombol jadi langsung buka setting
            requestButton.onClick.RemoveAllListeners();
            requestButton.gameObject.SetActive(false);
           // requestButton.onClick.AddListener(() => Application.OpenURL("app-settings:"));
        }
    }
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
    private IEnumerator RequestPermissionsAndroid()
    {
        UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Camera);
        UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Microphone);

        yield return new WaitForSeconds(1.2f);

        PlayerPrefs.SetInt("status_permission_key", 1);
        PlayerPrefs.Save();
        alreadyRequested = true;

        if (CheckPermissionGranted())
        {
            HidePermissionUI();
            OpenCame();
        }
        else
        {
            // Jika ditolak, tampilkan pesan dan disable tombol
            ShowPermissionUI("Camera and microphone access are required to analyze your emotions during gameplay. Please enable access in Settings to continue.");
           requestButton.onClick.RemoveAllListeners();
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
