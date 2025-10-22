using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

public class WebcamToRenderTexture : MonoBehaviour
{
    public enum CameraFacing { Front, Back }

    [Header("Target RenderTexture")]
    public RenderTexture targetTexture;

    [Header("Optional Preview")]
    public RawImage rawImage;
    public MeshRenderer meshRenderer;

    [Header("Camera Settings")]
    public CameraFacing preferredCamera = CameraFacing.Front;
    public int requestedWidth = 640;
    public int requestedHeight = 480;
    public int requestedFPS = 30;

    private WebCamTexture webcamTexture;
    private bool initialized = false;

    // === PUBLIC METHODS ===

    /// <summary>
    /// Start webcam manually.
    /// </summary>
    public void StartWebcam()
    {
        Debug.Log("Starwebcame xx");
        if (initialized)
        {
            Debug.Log("⚠ Webcam already running!");
            return;
        }

        StartCoroutine(StartWebcamRoutine());
    }

    /// <summary>
    /// Stop webcam manually.
    /// </summary>
    public void StopWebcam()
    {
        if (webcamTexture != null && webcamTexture.isPlaying)
        {
            webcamTexture.Stop();
            Debug.Log("🛑 Webcam stopped manually");
        }
        initialized = false;
    }

    // === PRIVATE COROUTINE ===

    private IEnumerator StartWebcamRoutine()
    {
        Debug.Log("📱 Initializing Webcam Script...");

        // Step 1: Request permission (Android only)
        yield return RequestAndroidPermission();

#if !UNITY_EDITOR && PLATFORM_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Debug.Log("❌ Camera permission denied!");
            yield break;
        }
#endif

        // Step 2: Verify target texture
        if (targetTexture == null)
        {
            Debug.LogError("❌ Target RenderTexture not assigned!");
            yield break;
        }

        // Step 3: Initialize webcam
        yield return InitializeWebcam();
    }

    private IEnumerator InitializeWebcam()
    {
        yield return new WaitForSeconds(0.1f);

        if (WebCamTexture.devices.Length == 0)
        {
            Debug.Log("⚠ No camera devices found!");
            yield break;
        }

        WebCamDevice? selectedDevice = WebCamTexture.devices
            .FirstOrDefault(d => d.isFrontFacing == (preferredCamera == CameraFacing.Front));

        selectedDevice ??= WebCamTexture.devices[0];

        Debug.Log($"📷 Using camera: {selectedDevice.Value.name}");

        webcamTexture = new WebCamTexture(selectedDevice.Value.name, requestedWidth, requestedHeight, requestedFPS);
        webcamTexture.Play();

        float timeout = 10f;
        while (timeout > 0f && webcamTexture.width < 100)
        {
            yield return null;
            timeout -= Time.deltaTime;
        }

        if (webcamTexture.width < 100)
        {
            Debug.LogError($"❌ Webcam failed to start (Timeout). Final width: {webcamTexture.width}");
            yield break;
        }

        Debug.Log($"✅ Webcam ready! Resolution: {webcamTexture.width}x{webcamTexture.height}");
        initialized = true;

        if (rawImage != null) rawImage.texture = targetTexture;
        if (meshRenderer != null) meshRenderer.material.mainTexture = targetTexture;
    }

    private void Update()
    {
        if (!initialized || !webcamTexture.didUpdateThisFrame)
            return;

        Graphics.Blit(webcamTexture, targetTexture);
    }

    private IEnumerator RequestAndroidPermission()
    {
#if !UNITY_EDITOR && PLATFORM_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Debug.Log("🔒 Requesting camera permission...");
            Permission.RequestUserPermission(Permission.Camera);

            float startTime = Time.time;
            while (!Permission.HasUserAuthorizedPermission(Permission.Camera) && Time.time < startTime + 10f)
            {
                yield return null;
            }
        }
#else
        yield return null;
#endif
    }
}
