using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;
using Unity.Sentis;
using MoodMe;
using DG.Tweening;

public class AngerEstimator : MonoBehaviour
{
    [Header("Input")]
    public RawImage inputRawImage; // Assign RawImage that contains the texture source

    [Header("Model Assets")]
    public ModelAsset faceDetector;
    public TextAsset anchorsCSV;

    [Header("Detection Settings")]
    public float iouThreshold = 0.3f;
    public float scoreThreshold = 0.5f;

    private const int detectorInputSize = 128;
    private const int k_NumAnchors = 896;

    private Worker worker;
    private float[,] anchors;
    private Tensor<float> detectorInput;
    private bool modelReady = false;
    public bool waiting = false;

    async void Start()
    {
        if (inputRawImage == null || inputRawImage.texture == null)
        {
            Debug.LogError("No RawImage texture assigned.");
            return;
        }

        // Load anchors and model
        anchors = BlazeUtils.LoadAnchors(anchorsCSV.text, k_NumAnchors);
        var model = ModelLoader.Load(faceDetector);

        // Build functional graph
        var graph = new FunctionalGraph();
        var input = graph.AddInput(model, 0);
        var outputs = Functional.Forward(model, 2 * input - 1);
        var boxes = outputs[0];
        var scores = outputs[1];

        // Prepare anchors tensor
        var anchorsData = new float[k_NumAnchors * 4];
        Buffer.BlockCopy(anchors, 0, anchorsData, 0, anchorsData.Length * sizeof(float));
        var anchorsTensor = Functional.Constant(new TensorShape(k_NumAnchors, 4), anchorsData);

        // Apply NMS
        var idx_scores_boxes = BlazeUtils.NMSFiltering(boxes, scores, anchorsTensor, detectorInputSize, iouThreshold, scoreThreshold);

        // Compile and create worker
        model = graph.Compile(idx_scores_boxes.Item1, idx_scores_boxes.Item2, idx_scores_boxes.Item3);
        worker = new Worker(model, BackendType.GPUCompute);

        detectorInput = new Tensor<float>(new TensorShape(1, detectorInputSize, detectorInputSize, 3));
        modelReady = true;

        Debug.Log("Face detector initialized.");

        // Invoke(nameof(_TestScan), 3f);
    }


    void _TestScan()
    {
        // TestScan();
    }

    public CanvasGroup loadingScanFace;
    // async void TestScan()
    // {
    //     loadingScanFace.DOFade(1, 0.5f);
    //     loadingScanFace.interactable = true;
    //     loadingScanFace.blocksRaycasts = true;
    //     await DetectFaceOnceAsync();
    //     loadingScanFace.DOFade(0, 0.5f).SetDelay(1f);
    //     loadingScanFace.interactable = false;
    //     loadingScanFace.blocksRaycasts = false;
    // }

    /// <summary>
    /// Detects face once from the current RawImage texture.
    /// Returns true if at least one face is found.
    /// </summary>
    public async Task<bool> DetectFaceOnceAsync()
    {

        if (!modelReady)
        {
            Debug.LogWarning("Model not ready yet.");
            return false;
        }

        if (inputRawImage == null || inputRawImage.texture == null)
        {
            Debug.LogWarning("No texture assigned to RawImage.");
            return false;
        }

        Texture texture = inputRawImage.texture;
        var size = Mathf.Max(texture.width, texture.height);
        var scale = size / (float)detectorInputSize;

        var M = BlazeUtils.mul(
            BlazeUtils.TranslationMatrix(0.5f * (new Vector2(texture.width, texture.height) + new Vector2(-size, size))),
            BlazeUtils.ScaleMatrix(new Vector2(scale, -scale))
        );

        // Prepare tensor
        BlazeUtils.SampleImageAffine(texture, detectorInput, M);
        worker.Schedule(detectorInput);

        // Yield satu frame agar GPU sempat selesai
        await Task.Yield();

        // Read model outputs (GPU → CPU)
        using var indices = (worker.PeekOutput(0) as Tensor<int>).ReadbackAndClone();
        using var scores = (worker.PeekOutput(1) as Tensor<float>).ReadbackAndClone();
        using var boxes = (worker.PeekOutput(2) as Tensor<float>).ReadbackAndClone();

        await Task.Yield(); // memberi waktu agar Unity tidak freeze

        bool faceFound = indices.shape.length > 0;
        waiting = true;
        return faceFound;
    }


    void OnDestroy()
    {
        worker?.Dispose();
        detectorInput?.Dispose();
    }
}
