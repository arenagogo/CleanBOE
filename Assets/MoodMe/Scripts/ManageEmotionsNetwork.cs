using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening; // perlu untuk RawImage

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
        public Unity.InferenceEngine.ModelAsset EmotionsNetwork;
        [Header("AngerEstimator")]
        public AngerEstimator angerEstimator;

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

        public float[] GetCurrentEmotionValues => DetectedEmotions.Values.ToArray();

        private Unity.InferenceEngine.Worker worker;
        private static Dictionary<string, float> DetectedEmotions;

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

        void Start()
        {
            var runtimeModel = Unity.InferenceEngine.ModelLoader.Load(EmotionsNetwork);
            worker = new Unity.InferenceEngine.Worker(runtimeModel, Unity.InferenceEngine.BackendType.GPUCompute);

            // init dict + inspector list
            DetectedEmotions = new Dictionary<string, float>();
            foreach (string key in EmotionsLabelFull)
            {
                DetectedEmotions.Add(key, 0);
                EmotionsInspector.Add(new EmotionValue { label = key, value = 0 });
            }

            if (sourceImage == null)
                Debug.LogWarning("⚠️ sourceImage belum di-assign, pastikan RawImage punya Texture.");
        }

        public void ScanFaceRemote()
        {
            RtmChannelManager.instant.GoScanFaceRemote();
        }



        public IEnumerator GetValue()
        {
            scaner.DOFade(1, 0.1f);
            scaning.SetActive(true);
            //  scanerAnim.StartLoop();
            int count = 0;
            bool proses = false;
            while (count < 50)
            {
                count++;
                yield return new WaitForSeconds(0.2f);
                proses = angerEstimator.DetectFaceOnce();
                if (proses)
                {
                    break;
                }
            }

            if (proses)
            {
                // Debug.Log("GetValue FaceAI");
                GetValueFaceAI();
            }
            else
            {
                AnimScore.Instance.SendDataToRTM(0, 0, 0, 0, 0, 0, 0, false);
            }
            scaner.DOFade(0, 0.1f);
            scaning.SetActive(false);
            //  scanerAnim.StopLoop();
        }

        public void GetValueFaceAI()
        {
            if (sourceImage == null || sourceImage.texture == null)
            {
                Debug.LogError("❌ RawImage belum ada Texture!");
                return;
            }

            Texture srcTex = sourceImage.texture;
            Texture2D tex;

            // kalau texture sudah Texture2D langsung cast
            if (srcTex is Texture2D)
            {
                tex = UnityEngine.Object.Instantiate(srcTex) as Texture2D;
            }
            // kalau texture ternyata RenderTexture, convert dulu ke Texture2D
            else if (srcTex is RenderTexture rt)
            {
                RenderTexture.active = rt;
                tex = new Texture2D(ImageNetworkWidth, ImageNetworkHeight, TextureFormat.R8, false);
                tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                tex.Apply();
                RenderTexture.active = null;
            }
            else
            {
                Debug.LogError("❌ Source texture di RawImage bukan Texture2D atau RenderTexture.");
                return;
            }

            try
            {
                // ambil pixel grayscale
                Color32[] pixels = tex.GetPixels32();
                float[] inputArray = new float[ImageNetworkWidth * ImageNetworkHeight];

                for (int y = 0; y < ImageNetworkHeight; y++)
                {
                    for (int x = 0; x < ImageNetworkWidth; x++)
                    {
                        int idx = y * ImageNetworkWidth + x;
                        float gray = pixels[idx].r; // grayscale dari channel R

                        if (NormalizeToMinusOneToOne)
                            inputArray[idx] = (gray - 127.5f) / 127.5f; // [-1,1]
                        else
                            inputArray[idx] = gray / 255f; // [0,1]
                    }
                }

                // shape NCHW: [1,1,48,48]
                var shape = new Unity.InferenceEngine.TensorShape(1, ChannelCount, ImageNetworkHeight, ImageNetworkWidth);

                using (var inputTensor = new Unity.InferenceEngine.Tensor<float>(shape, inputArray))
                {
                    worker.Schedule(inputTensor);

                    using (var outputTensor = worker.PeekOutput() as Unity.InferenceEngine.Tensor<float>)
                    {
                        if (outputTensor == null)
                        {
                            Debug.LogError("❌ Output tensor null, inference gagal.");
                            return;
                        }

                        using (var clonedTensor = outputTensor.ReadbackAndClone())
                        {
                            float[] results = clonedTensor.AsReadOnlyNativeArray().ToArray();

                            // Softmax normalisasi
                            float maxLogit = results.Max();
                            float sumExp = results.Sum(v => Mathf.Exp(v - maxLogit));
                            for (int i = 0; i < results.Length; i++)
                            {
                                results[i] = Mathf.Exp(results[i] - maxLogit) / sumExp;
                            }

                            int count = Mathf.Min(results.Length, EmotionsLabelFull.Length);

                            for (int i = 0; i < count; i++)
                            {
                                string label = EmotionsLabelFull[i];
                                DetectedEmotions[label] = results[i];
                            }

                            // update inspector
                            for (int j = 0; j < EmotionsInspector.Count; j++)
                            {
                                string key = EmotionsInspector[j].label;
                                if (DetectedEmotions.ContainsKey(key))
                                    EmotionsInspector[j].value = DetectedEmotions[key];
                            }

                            // ✅ Panggil SendScore setelah hasil siap
                            SendScore();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Error saat proses GetValue: {ex.Message}");
            }
            finally
            {
                Destroy(tex);
            }
        }

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
