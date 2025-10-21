using DG.Tweening; // pastikan kamu sudah import DOTween
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LineRenderer))]
public class HeartRateVisualizer : MonoBehaviour
{
    [Header("💓 Hyperate / BPM Settings")]
    [Tooltip("Masukkan BPM manual atau dari Hyperate API")]
    public float bpm = 80f;                // Bisa diubah runtime
    public float amplitude = 1.0f;         // Tinggi gelombang (spike)
    public int resolution = 100;           // Jumlah titik
    public float lineWidth = 0.25f;        // Ketebalan garis tetap

    [Header("🎨 Warna Dinamis dari BPM")]
    public Material lineMaterial;
    public Color baseColor = Color.green;
    public Color peakColor = Color.red;
    [Range(60f, 200f)] public float colorChangeThreshold = 120f;

    [Header("🩺 Visual Settings")]
    public float lineLength = 10f;
    public float animationSpeed = 2.0f;

    [Header("❤️ Heart Image (UI Detak)")]
    public Image heartImg;
    public float heartBeatScale = 1.2f; // seberapa besar membesar saat detak

    private LineRenderer lineRenderer;
    private float time;
    private Tween heartTween; // reference animasi aktif
    public CanvasGroup cvs;

    private Vector3 curPos;
    public TextMeshPro heartText;


    void Awake()
    {
        curPos = transform.localPosition;
    }


    private void OnEnable()
    {
        // Daftar event listener
        HyperRateManager.OnBPMChanged += HandleBPMChanged;
    }

    private void OnDisable()
    {
        // Hapus listener saat script dimatikan, biar gak memory leak
        HyperRateManager.OnBPMChanged -= HandleBPMChanged;
    }

    private void HandleBPMChanged(string _bpm)
    {
        if (GlobalVariable.gamemode == GlobalVariable.GAMEMODE.SMARTWACTH)
        {
            if (float.TryParse(_bpm, out float bpmValue))
            {
                bpm = bpmValue;
            }
            else
            {
                Debug.LogWarning($"⚠️ BPM tidak valid: {bpm}");
            }
        }

    }

    public void SetupGameMode()
    {
        // StartHeartBeat();
    }

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = resolution;
        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = false;
        lineRenderer.widthMultiplier = lineWidth;

        if (lineMaterial != null)
            lineRenderer.material = lineMaterial;
        else
            Debug.LogWarning("⚠️ LineRenderer belum punya material, warna tidak akan tampil!");

        StartHeartBeat(); // mulai animasi detak

    }

    void Update()
    {
        if (GlobalVariable.gamemode != GlobalVariable.GAMEMODE.SMARTWACTH)
        {
            cvs.alpha = 0;
            transform.localPosition = curPos * 100;
            return;
        }
        transform.localPosition = curPos;
        //  gameObject.SetActive(true);
        cvs.alpha = 1;
        // waktu animasi stabil, tidak dipengaruhi BPM
        time += Time.deltaTime * animationSpeed;
        float frequency = bpm / 60f;

        // 🎨 Warna dinamis berdasarkan BPM
        Color dynamicColor = Color.Lerp(baseColor, peakColor, Mathf.InverseLerp(60f, colorChangeThreshold, bpm));
        if (lineRenderer.material != null)
        {
            Material mat = lineRenderer.material;
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", dynamicColor);
            else if (mat.HasProperty("_MainColor"))
                mat.SetColor("_MainColor", dynamicColor);
            else if (mat.HasProperty("_TintColor"))
                mat.SetColor("_TintColor", dynamicColor);
            else if (mat.HasProperty("_Color"))
                mat.SetColor("_Color", dynamicColor);
        }

        heartText.color = dynamicColor;

        // 💓 Bentuk gelombang heartbeat
        for (int i = 0; i < resolution; i++)
        {
            float progress = (float)i / (resolution - 1);
            float x = progress * lineLength;
            float wave = Mathf.Sin(x * frequency + time);
            float spike = Mathf.Pow(Mathf.Abs(wave), 20f) * amplitude;
            float y = Mathf.Sin(x * frequency + time) * amplitude * 0.2f + spike;

            lineRenderer.SetPosition(i, new Vector3(x - lineLength / 2f, y, 0f));
        }

        heartText.text = bpm.ToString();

    }

    void StartHeartBeat()
    {
        // Hentikan tween sebelumnya biar gak dobel
        if (heartTween != null && heartTween.IsActive())
            heartTween.Kill();
        if (bpm <= 60)
        {
            bpm = 60f;
        }

        float beatInterval = 120f / bpm;
        // Reset skala ke normal
        heartImg.transform.localScale = Vector3.one;

        // Tween loop bolak-balik tanpa henti
        heartTween = heartImg.transform
            .DOScale(heartBeatScale, beatInterval / 2f)
            .SetEase(Ease.OutQuad).OnStepComplete(() => { StartHeartBeat(); });
    }


}
