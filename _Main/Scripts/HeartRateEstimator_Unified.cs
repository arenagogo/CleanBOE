using System;
using System.Collections;
using System.Runtime.InteropServices;
using DG.Tweening;
using MoodMe;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering; // <-- Tambahkan ini
using UnityEngine.UI;

public class HeartRateEstimator_Unified : MonoBehaviour
{
    [Header("Link Ke Controller (WAJIB)")]
    public SimpleAgoraController_Unified controller;
    // public EmotionsManager emotionsManager;

    [Header("Video Source (WAJIB)")]
    public RenderTexture face;

    [Header("Preview UI")]
    public RawImage unifiedPreview;
    public AspectRatioFitter aspectFitter;

    [Header("UI BPM")]
    public TextMeshProUGUI bpmText;
    public TextMeshProUGUI debugText;
    public Image slidBpm;

    // Private
    private Color32[] pixelBuffer;
    private byte[] rgbaBuffer;
    // private int _gimmickBPM = 75;
    [SerializeField] private int bpmFromSmartWacth = 0;
    [SerializeField] private bool isVideoActive = false;

    // Variabel baru untuk Async Readback
    private bool isProcessingFrame = false;

    // public HeartRateResultAnalyzer heartRateResultAnalyzer;

    private void OnEnable()
    {
        HyperRateManager.OnBPMChanged += OnBPMChangedFromSmartWatch;
    }

    private void OnDisable()
    {
        HyperRateManager.OnBPMChanged -= OnBPMChangedFromSmartWatch;
    }

    private void OnDestroy()
    {
        StopVideo();
    }

    private void Update()
    {
        if (isVideoActive && !isProcessingFrame)
        {
            // Minta data frame dari GPU secara asinkron
            UpdateAndPushVideoFrameAsync();
        }
    }

    //================================================================================
    // CONTROL
    //================================================================================

    public void StartVideo()
    {
        if (face == null)
        {
            Debug.LogError("[HR Simulator] RenderTexture (face) belum di-assign!");
            return;
        }

        if (unifiedPreview != null)
        {
            unifiedPreview.texture = face;
        }
        if (aspectFitter != null)
        {
            aspectFitter.aspectRatio = (float)face.width / face.height;
        }

        if (bpmText != null) bpmText.text = "--";
        if (debugText != null) debugText.text = "VIDEO READY";

        // Inisialisasi buffer sekali saja di sini
        int w = face.width;
        int h = face.height;
        if (pixelBuffer == null || pixelBuffer.Length != w * h)
            pixelBuffer = new Color32[w * h];
        if (rgbaBuffer == null || rgbaBuffer.Length != w * h * 4)
            rgbaBuffer = new byte[w * h * 4];

        isVideoActive = true;
        StartCoroutine(UpdateBPMRoutine());
    }

    public void StopVideo()
    {
        isVideoActive = false;
        isProcessingFrame = false; // Hentikan proses yang mungkin berjalan

        if (unifiedPreview != null) unifiedPreview.texture = null;
        if (debugText != null) debugText.text = "VIDEO OFF";

        StopAllCoroutines();
    }

    //================================================================================
    // VIDEO PROCESSING (REVISED WITH ASYNC METHOD)
    //================================================================================

    private void UpdateAndPushVideoFrameAsync()
    {
        if (face == null) return;

        isProcessingFrame = true;
        AsyncGPUReadback.Request(face, 0, TextureFormat.RGBA32, OnCompleteReadback);
    }

    void OnCompleteReadback(AsyncGPUReadbackRequest request)
    {
        if (!isVideoActive)
        {
            isProcessingFrame = false;
            return;
        }

        if (request.hasError)
        {
            Debug.LogError("GPU readback error detected.");
            isProcessingFrame = false;
            return;
        }

        // Dapatkan data piksel langsung ke buffer Color32[]
        var data = request.GetData<Color32>();
        data.CopyTo(pixelBuffer);

        // Salin data ke byte buffer untuk Agora
        GCHandle handle = GCHandle.Alloc(pixelBuffer, GCHandleType.Pinned);
        try
        {
            IntPtr ptr = handle.AddrOfPinnedObject();
            Marshal.Copy(ptr, rgbaBuffer, 0, rgbaBuffer.Length);
        }
        finally
        {
            if (handle.IsAllocated) handle.Free();
        }

        // Kirim frame ke Agora
        if (controller != null)
        {
            controller.PushExternalVideoFrame(rgbaBuffer, face.width, face.height);
        }

        // Siap untuk memproses frame berikutnya
        isProcessingFrame = false;
    }


    //================================================================================
    // BPM LOGIC (Tidak ada perubahan)
    //================================================================================
    public EffectManager effectManager;
    private IEnumerator UpdateBPMRoutine()
    {
        while (isVideoActive)
        {
            yield return new WaitForSeconds(3);
            if (GlobalVariable.gamemode == GlobalVariable.GAMEMODE.SMARTWACTH)
            {
                RtmChannelManager.instant.DeviceInfoLocal("Smartwatch");
                RtmChannelManager.instant.SenDevice("Smartwatch", controller.battleId);
                controller.SendChatText($"mamahjahat{bpmFromSmartWacth}");
                // controller.SendChatText($"mamahjahat{emotSementara}");

                if (bpmText != null)
                {
                    bpmText.text = bpmFromSmartWacth.ToString();
                    //  bpmText.text = emotSementara.ToString();
                }


                // float targetFill = Mathf.Clamp01((float)bpmFromSmartWacth / GlobalVariable.maxHeartRate);
                float targetFill = Mathf.Clamp01((float)emotSementara / GlobalVariable.maxHeartRate);
                GlobalVariable.BPM = (int)targetFill;
                effectManager.SetValue(targetFill);
                if (slidBpm != null) slidBpm.DOFillAmount(targetFill, 0.5f);
                GameScoreManager.instance.heartRateData.Add(bpmFromSmartWacth);
                // GameScoreManager.instance.heartRateData.Add(emotSementara);


                GameScoreManager.instance.UpdateBPMLocal(bpmFromSmartWacth);
                // GameScoreManager.instance.UpdateBPMLocal(emotSementara);
            }
            else
            {
                // BPM gimmick = nilai dari EmotionsManager
                // float emot = emotionsManager.Surprised * 100;
                // float emot = emotionsManager.Surprised;
                // _gimmickBPM = 75 + (int)emot; // Contoh: detak jantung dasar 75 + efek emosi

                // RtmChannelManager.instant.DeviceInfoLocal("Face Mode");
                // RtmChannelManager.instant.SenDevice("Face Mode", controller.battleId);
                // controller.SendChatText($"mamahjahat{emot}");

                // if (bpmText != null) bpmText.text = emot.ToString("F0");
                // float targetFill = Mathf.Clamp01((float)emot / GlobalVariable.maxHeartRate);
                // GlobalVariable.BPM = (int)targetFill;
                // effectManager.SetValue(emot);
                // if (slidBpm != null) slidBpm.DOFillAmount(targetFill, 0.5f);
                // GameScoreManager.instance.heartRateData.Add((int)emot);
            }


        }
    }

    public int emotSementara = 40;
    //public EmotionsManager emotionsManager;

    private void OnBPMChangedFromSmartWatch(string bpmString)
    {
        if (int.TryParse(bpmString, out int bpm))
        {
            bpmFromSmartWacth = bpm;
        }
    }


}
