using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening; // perlu untuk RawImage
using Unity.Sentis;
using UnityEngine;
using UnityEngine.UI;

namespace MoodMe
{
    [Serializable]
    public class EmotionValue
    {
        public string label;
        [Range(0f, 1f)] public float value;
    }

    public class ManageEmotionsNetwork : MonoBehaviour
    {
        public GameObject scaning;
        public static ManageEmotionsNetwork instant;
        [Header("Model")]
        public ModelAsset EmotionsNetwork;
        [Header("AngerEstimator")]
        // public AngerEstimator angerEstimator;

        [Header("Input RawImage")]
        public RawImage sourceImage; // ganti dari RenderTexture ke RawImage

        [Header("Input Settings")]
        public int ImageNetworkWidth = 48;
        public int ImageNetworkHeight = 48;
        [Range(1, 4)] public int ChannelCount = 1;
        public bool Process = true;

        [Header("Preprocessing")]
        public bool NormalizeToMinusOneToOne = false; // toggle di Inspector

        [Header("Output (Inspector View)")]
        public List<EmotionValue> EmotionsInspector = new List<EmotionValue>();

        public float[] GetCurrentEmotionValues => (DetectedEmotions != null) ? DetectedEmotions.Values.ToArray() : Array.Empty<float>();

        private Worker worker;
        // initialize to avoid null-ref if property accessed early
        private static Dictionary<string, float> DetectedEmotions = new Dictionary<string, float>();

        private Tensor<float> inputTensor;
        // private readonly string[] EmotionsLabelFull = { ... };
        // private Worker worker;

        public Image scaner;
        //  public ScanerAnim scanerAnim;

        // Label sesuai urutan FER2013 (0=Angry ... 6=Neutral)
        private readonly string[] EmotionsLabelFull =
            { "Angry", "Disgust", "Fear", "Happy", "Sad", "Surprise", "Neutral" };


        void Awake()
        {
            instant = this;
            scaner.DOFade(0, 0);
        }

        [Obsolete]
        void Start()
        {
            var runtimeModel = ModelLoader.Load(EmotionsNetwork);
            worker = new Worker(runtimeModel, BackendType.GPUCompute);

            // init dict + inspector list
            DetectedEmotions = new Dictionary<string, float>();
            foreach (string key in EmotionsLabelFull)
            {
                DetectedEmotions.Add(key, 0);
                EmotionsInspector.Add(new EmotionValue { label = key, value = 0 });
            }

            if (sourceImage == null)
                Debug.LogWarning("⚠️ sourceImage belum di-assign, pastikan RawImage punya Texture.");

            if (GlobalVariable.OnInitFaceScane == false)
            {
                StartCoroutine(GetValueFaceAIAsync());
                Debug.Log("INI AWAL");
            }

        }

        public void ScanFaceRemote()
        {
            RtmChannelManager.instant.GoScanFaceRemote();
        }

        [Obsolete]

        public IEnumerator GetValue()
        {
            scaner.DOFade(1, 0.1f);
            scaning.SetActive(true);

            // tunggu sebentar supaya animasi fade sempat tampil
            yield return new WaitForSeconds(0.2f);

            // Jalankan inference async tanpa freeze
            yield return StartCoroutine(GetValueFaceAIAsync());

            scaner.DOFade(0, 0.1f);
            scaning.SetActive(false);
        }

        [Obsolete]
        private IEnumerator GetValueFaceAIAsync()
        {
            if (sourceImage == null || sourceImage.texture == null)
            {
                Debug.LogError("❌ RawImage belum ada Texture!");
                yield break;
            }

            // 1. Tentukan parameter normalisasi
            // (Nilai ini diambil dari 'NormalizeToMinusOneToOne' di Inspector)
            float meanValue = NormalizeToMinusOneToOne ? 127.5f : 0.0f;
            float stdValue = NormalizeToMinusOneToOne ? 127.5f : 255.0f;

            // 2. Buat/reuse tensor input (NCHW)
            var shape = new TensorShape(1, ChannelCount, ImageNetworkHeight, ImageNetworkWidth);
            if (inputTensor == null)
                inputTensor = new Tensor<float>(shape);

            // 3. KONVERSI TEKSTUR → Tensor (GPU path when available)
            // Build a TextureTransform using the public fluent API (SetDimensions) and reuse the preallocated inputTensor.
            var transform = new TextureTransform().SetDimensions(ImageNetworkWidth, ImageNetworkHeight, ChannelCount);
            // Note: we don't set internal channelScale/bias here; Sentis will infer reasonable defaults.

            // Perform conversion directly into the preallocated inputTensor (avoids allocations and uses GPU when possible)
            TextureConverter.ToTensor(sourceImage.texture, inputTensor, transform);

            // 4. JALANKAN MODEL
            worker.Schedule(inputTensor);

            // 5. TUNGGU HASIL (ASYNC)
            yield return new WaitUntil(() => worker.PeekOutput() != null);

            // 6. BACA HASIL
            using (var outputTensor = worker.PeekOutput() as Tensor<float>)
            {
                if (outputTensor == null)
                {
                    Debug.LogError("❌ Output tensor null, inference gagal.");
                    yield break;
                }

                // Ambil data dari GPU
                using (var clonedTensor = outputTensor.ReadbackAndClone())
                {
                    float[] results = clonedTensor.AsReadOnlyNativeArray().ToArray();

                    // 7. SOFTMAX
                    float maxLogit = results.Max();
                    float sumExp = results.Sum(v => Mathf.Exp(v - maxLogit));
                    for (int i = 0; i < results.Length; i++)
                        results[i] = Mathf.Exp(results[i] - maxLogit) / sumExp;

                    // 8. SIMPAN HASIL
                    int count = Mathf.Min(results.Length, EmotionsLabelFull.Length);
                    for (int i = 0; i < count; i++)
                        DetectedEmotions[EmotionsLabelFull[i]] = results[i];

                    for (int j = 0; j < EmotionsInspector.Count; j++)
                        EmotionsInspector[j].value = DetectedEmotions[EmotionsInspector[j].label];

                    // 9. KIRIM HASIL
                    if (GlobalVariable.OnInitFaceScane)
                        SendScore();

                    GlobalVariable.OnInitFaceScane = true;
                }
            }
            // Tidak ada lagi 'Destroy(tex)', jadi tidak ada GC spike!
        }


        // public void GetValueFaceAI()
        // {
        //     if (sourceImage == null || sourceImage.texture == null)
        //     {
        //         Debug.LogError("❌ RawImage belum ada Texture!");
        //         return;
        //     }

        //     Texture srcTex = sourceImage.texture;
        //     Texture2D tex;

        //     // kalau texture sudah Texture2D langsung cast
        //     if (srcTex is Texture2D)
        //     {
        //         tex = UnityEngine.Object.Instantiate(srcTex) as Texture2D;
        //     }
        //     // kalau texture ternyata RenderTexture, convert dulu ke Texture2D
        //     else if (srcTex is RenderTexture rt)
        //     {
        //         RenderTexture.active = rt;
        //         tex = new Texture2D(ImageNetworkWidth, ImageNetworkHeight, TextureFormat.R8, false);
        //         tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        //         tex.Apply();
        //         RenderTexture.active = null;
        //     }
        //     else
        //     {
        //         Debug.LogError("❌ Source texture di RawImage bukan Texture2D atau RenderTexture.");
        //         return;
        //     }

        //     try
        //     {
        //         // ambil pixel grayscale
        //         Color32[] pixels = tex.GetPixels32();
        //         float[] inputArray = new float[ImageNetworkWidth * ImageNetworkHeight];

        //         for (int y = 0; y < ImageNetworkHeight; y++)
        //         {
        //             for (int x = 0; x < ImageNetworkWidth; x++)
        //             {
        //                 int idx = y * ImageNetworkWidth + x;
        //                 float gray = pixels[idx].r; // grayscale dari channel R

        //                 if (NormalizeToMinusOneToOne)
        //                     inputArray[idx] = (gray - 127.5f) / 127.5f; // [-1,1]
        //                 else
        //                     inputArray[idx] = gray / 255f; // [0,1]
        //             }
        //         }

        //         // shape NCHW: [1,1,48,48]
        //         var shape = new TensorShape(1, ChannelCount, ImageNetworkHeight, ImageNetworkWidth);

        //         using (var inputTensor = new Tensor<float>(shape, inputArray))
        //         {
        //             worker.Schedule(inputTensor);

        //             using (var outputTensor = worker.PeekOutput() as Tensor<float>)
        //             {
        //                 if (outputTensor == null)
        //                 {
        //                     Debug.LogError("❌ Output tensor null, inference gagal.");
        //                     return;
        //                 }

        //                 using (var clonedTensor = outputTensor.ReadbackAndClone())
        //                 {
        //                     float[] results = clonedTensor.AsReadOnlyNativeArray().ToArray();

        //                     // Softmax normalisasi
        //                     float maxLogit = results.Max();
        //                     float sumExp = results.Sum(v => Mathf.Exp(v - maxLogit));
        //                     for (int i = 0; i < results.Length; i++)
        //                     {
        //                         results[i] = Mathf.Exp(results[i] - maxLogit) / sumExp;
        //                     }

        //                     int count = Mathf.Min(results.Length, EmotionsLabelFull.Length);

        //                     for (int i = 0; i < count; i++)
        //                     {
        //                         string label = EmotionsLabelFull[i];
        //                         DetectedEmotions[label] = results[i];
        //                     }

        //                     // update inspector
        //                     for (int j = 0; j < EmotionsInspector.Count; j++)
        //                     {
        //                         string key = EmotionsInspector[j].label;
        //                         if (DetectedEmotions.ContainsKey(key))
        //                             EmotionsInspector[j].value = DetectedEmotions[key];
        //                     }

        //                     // ✅ Panggil SendScore setelah hasil siap
        //                     SendScore();
        //                 }
        //             }
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         Debug.LogError($"❌ Error saat proses GetValue: {ex.Message}");
        //     }
        //     finally
        //     {
        //         Destroy(tex);
        //     }
        // }

        // 🔹 Kirim hasil emosi ke sistem lain (misal UI)
        void SendScore()
        {
            float angry = EmotionsInspector[0].value * 100;
            float disgust = EmotionsInspector[1].value * 100;
            float fear = EmotionsInspector[2].value * 100;
            float happy = EmotionsInspector[3].value * 100;
            float sad = EmotionsInspector[4].value * 100;
            float surprise = EmotionsInspector[5].value * 100;
            float neutral = EmotionsInspector[6].value * 100;

            // contoh: kirim ke animasi atau UI
            if (GlobalVariable.gamemode == GlobalVariable.GAMEMODE.FACEMODE)
                AnimScore.Instance.SendDataToRTM(angry, disgust, fear, happy, sad, surprise, neutral, true);

            // Debug.Log($"✅ Emotion sent → Angry:{angry:F2}, Sad:{sad:F2}, Neutral:{neutral:F2}");
        }

        private void OnDisable()
        {
            worker?.Dispose();
            worker = null;
        }

        private void OnDestroy()
        {
            worker?.Dispose();
        }
    }
}