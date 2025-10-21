using System;
using System.Collections;
using Unity.InferenceEngine;
using UnityEngine;

namespace MoodMe
{


    public class MoodMeProccessing : MonoBehaviour
    {
        [SerializeField] private Texture2D inputTexture;
        [SerializeField] private Unity.InferenceEngine.ModelAsset modelAsset;
        [SerializeField] private float[] outputValues;

        private Unity.InferenceEngine.Worker worker;
        private Tensor input;

        void OnEnable()
        {
            var runtimeModel = Unity.InferenceEngine.ModelLoader.Load(modelAsset);
            worker = new Unity.InferenceEngine.Worker(runtimeModel, Unity.InferenceEngine.BackendType.GPUCompute);
            Debug.Log("Model loaded and worker created with GPUCompute backend.");
        }

        void ExecuteModel()
        {
            input?.Dispose();

            var transform = new Unity.InferenceEngine.TextureTransform().SetTensorLayout(Unity.InferenceEngine.TensorLayout.NHWC).SetDimensions(48, 48, 1);
            input = Unity.InferenceEngine.TextureConverter.ToTensor(inputTexture, transform);

            worker.Schedule(input);

            var outputTensor = worker.PeekOutput() as Unity.InferenceEngine.Tensor<float>;

            if (outputTensor == null)
            {
                Debug.LogError("Output tensor is null. Please check model configuration and input compatibility.");
                return;
            }

            Debug.Log("Output Tensor Shape: " + outputTensor.shape);

            var cpuTensor = outputTensor.ReadbackAndClone();

            // Dynamically resize outputValues if necessary
            var outputArray = cpuTensor.AsReadOnlyNativeArray();
            if (outputValues == null || outputValues.Length != outputArray.Length)
            {
                outputValues = new float[outputArray.Length];
            }

            int length = Mathf.Min(outputValues.Length, outputArray.Length);
            for (int i = 0; i < length; i++)
            {
                outputValues[i] = outputArray[i];
                Debug.Log($"Output value at index {i}: {outputValues[i]}");
            }

            cpuTensor.Dispose();
            outputTensor.Dispose();
        }

        void Update()
        {
            // Execute the model once per frame
            ExecuteModel();
        }

        void OnDisable()
        {
            worker?.Dispose();
            input?.Dispose();

            worker = null;
            input = null;

            // If outputValues is serialized, clear it to avoid Editor holding onto it
            if (outputValues != null)
            {
                Array.Clear(outputValues, 0, outputValues.Length);
            }

            Debug.Log("Worker and input tensor disposed. Output values cleared.");
        }
    }
}
