using UnityEngine;

public class HeartRateSimulator : MonoBehaviour
{
    [Header("Referensi")]
    public HeartRateDisplay heartRateDisplay; // Seret GameObject HeartRateMonitor ke sini

    [Header("Simulasi Detak Jantung")]
    public float timeBetweenBeats = 1.0f; // Detak setiap 1 detik
    public float timeBetweenSamples = 0.02f; // Seberapa cepat data diperbarui

    // Pola untuk satu kali detak (puncak QRS)
    private readonly float[] beatPattern = { 0.1f, 0.2f, 1.0f, 2.5f, -0.8f, 0.3f, 0.1f, 0f };
    private int beatIndex = -1;
    private float lastBeatTime;
    private float lastSampleTime;

    void Update()
    {
        // Memicu detak jantung baru secara berkala
        if (Time.time - lastBeatTime > timeBetweenBeats)
        {
            lastBeatTime = Time.time;
            beatIndex = 0; // Mulai pola detak
        }

        // Memperbarui nilai grafik secara berkala
        if (Time.time - lastSampleTime > timeBetweenSamples)
        {
            lastSampleTime = Time.time;
            float newValue = 0f;

            // Jika sedang dalam pola detak, gunakan nilai dari pola
            if (beatIndex != -1)
            {
                newValue = beatPattern[beatIndex];
                beatIndex++;
                if (beatIndex >= beatPattern.Length)
                {
                    beatIndex = -1; // Selesai
                }
            }
            // Jika tidak, beri sedikit noise acak agar tidak terlalu datar
            else
            {
                newValue = Random.Range(-0.05f, 0.05f);
            }

            // Kirim nilai baru ke skrip display
            heartRateDisplay.UpdateValue(newValue);
        }
    }
}