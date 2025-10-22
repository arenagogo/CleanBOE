using UnityEngine;
using UnityEngine.UI;

public class SimpleWebcamTest : MonoBehaviour
{
    public RawImage displayArea;
    private WebCamTexture webcamTexture;

    void Start()
    {
        if (displayArea == null)
        {
            Debug.LogError("Error: RawImage displayArea belum dihubungkan!");
            return;
        }

        // Cek apakah ada kamera yang tersedia
        if (WebCamTexture.devices.Length == 0)
        {
            Debug.LogError("Tidak ada webcam yang ditemukan!");
            return;
        }

        // Mulai menggunakan kamera default
        webcamTexture = new WebCamTexture();
        displayArea.texture = webcamTexture;
        displayArea.material.mainTexture = webcamTexture; // Pastikan material juga di-set
        webcamTexture.Play();

        Debug.Log("Webcam seharusnya sudah aktif.");
    }

    void OnDestroy()
    {
        // Pastikan kamera berhenti saat script dihancurkan
        if (webcamTexture != null && webcamTexture.isPlaying)
        {
            webcamTexture.Stop();
        }
    }
}