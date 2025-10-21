using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class HeartRateDisplay : MonoBehaviour
{
    [Header("Referensi")]
    private LineRenderer lineRenderer;

    [Header("Pengaturan Grafik")]
    [Tooltip("Jumlah total titik yang ditampilkan di layar.")]
    public int pointCount = 100;

    [Tooltip("Jarak horizontal antar titik.")]
    public float xSpacing = 0.1f;

    [Tooltip("Pengali untuk tinggi gelombang (amplitudo).")]
    public float amplitude = 2f;

    // Antrian untuk menyimpan nilai Y (ketinggian) dari setiap titik
    private Queue<float> yValues;

    void Awake()
    {
        // Ambil komponen LineRenderer saat mulai
        lineRenderer = GetComponent<LineRenderer>();
        InitializeGraph();
    }

    /// <summary>
    /// Menyiapkan grafik dengan garis lurus di awal.
    /// </summary>
    void InitializeGraph()
    {
        // Atur jumlah titik pada LineRenderer
        lineRenderer.positionCount = pointCount;

        // Buat antrian baru untuk nilai Y
        yValues = new Queue<float>();

        // Isi antrian dengan nilai awal 0 (garis lurus)
        for (int i = 0; i < pointCount; i++)
        {
            yValues.Enqueue(0f);
        }

        // Gambar grafik awal
        DrawGraph();
    }

    /// <summary>
    // Ini adalah fungsi utama yang Anda panggil dari skrip lain
    // untuk memasukkan data detak jantung secara realtime.
    /// </summary>
    /// <param name="newValue">Angka baru yang akan ditampilkan.</param>
    public void UpdateValue(float newValue)
    {
        if (yValues == null) return;

        // Buang nilai paling lama (paling kiri)
        yValues.Dequeue();

        // Tambahkan nilai baru (paling kanan)
        yValues.Enqueue(newValue);

        // Gambar ulang grafik dengan data terbaru
        DrawGraph();
    }

    /// <summary>
    /// Menggambar ulang semua titik pada LineRenderer.
    /// </summary>
    void DrawGraph()
    {
        // Buat array sementara untuk menampung posisi Vector3
        Vector3[] positions = new Vector3[pointCount];
        float[] currentYValues = yValues.ToArray();

        for (int i = 0; i < pointCount; i++)
        {
            // Hitung posisi setiap titik
            // X: Berdasarkan indeks dan jarak (spacing)
            // Y: Berdasarkan nilai dari antrian dikali amplitudo
            float xPos = i * xSpacing;
            float yPos = currentYValues[i] * amplitude;

            positions[i] = new Vector3(xPos, yPos, 0f);
        }

        // Terapkan semua posisi baru ke LineRenderer
        lineRenderer.SetPositions(positions);
    }
}