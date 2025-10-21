using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.ObjdetectModule;
using OpenCVForUnity.UnityUtils;
using UnityEngine;
using UnityEngine.UI;
// Alias agar Rect dari OpenCV tidak bentrok dengan UnityEngine.Rect
using CvRect = OpenCVForUnity.CoreModule.Rect;

public class AngerEstimator : MonoBehaviour
{
    [Header("Input")]
    public RawImage inputRawImage; // Input dari RawImage (misal webcam)

    [Header("Preview (Optional)")]
    public RawImage preview; // Preview kotak wajah

    [Header("Cascade File")]
    public string faceCascadeFile = "lbpcascade_frontalface.xml"; // LBP lebih cepat, bisa diganti Haar

    [Header("Face Detection Settings")]
    [Range(1.01f, 2f)] public float faceScaleFactor = 1.1f;
    [Range(1, 10)] public int faceMinNeighbors = 3;
    public Vector2Int faceMinSize = new Vector2Int(80, 80);
    public Vector2Int faceMaxSize = new Vector2Int(0, 0);

    [Header("Debug")]
    public bool drawDebug = true;

    // Runtime
    private CascadeClassifier faceCascade;
    private Mat frameMat, grayMat, resizedGrayMat;
    private Texture2D tempTex;
    private MatOfRect faces; // reusable buffer

    public bool testing;
    public Vector3 facePosition;
    public float dikali;
    public Vector3 facePositionx;

    public GameObject kubus;

    void Start()
    {
        // Load cascade
        string facePath = Utils.getFilePath(faceCascadeFile);
        faceCascade = new CascadeClassifier(facePath);

        if (faceCascade.empty())
        {
            Debug.LogError("❌ Gagal memuat cascade file!");
            enabled = false;
            return;
        }

        if (inputRawImage == null || inputRawImage.texture == null)
        {
            Debug.LogError("❌ RawImage input belum diassign atau belum memiliki texture!");
            enabled = false;
            return;
        }

        // Inisialisasi buffer Mat & Texture
        Texture tex = inputRawImage.texture;
        tempTex = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);
        frameMat = new Mat(tex.height, tex.width, CvType.CV_8UC3);
        grayMat = new Mat(tex.height, tex.width, CvType.CV_8UC1);
        resizedGrayMat = new Mat(tex.height / 2, tex.width / 2, CvType.CV_8UC1);
        faces = new MatOfRect();

        if (preview != null)
            preview.texture = tempTex;

        // StarFaceFollow();
    }


    // void StarFaceFollow()
    // {
    //     DetectFaceOnce();
    //     Vector3 pos = facePosition * dikali;
    //     Vector3 pos1 = pos + facePositionx;
    //     kubus.transform.DOMove(pos1, 0.3f).OnComplete(() =>
    //     {
    //         StarFaceFollow();
    //     });
    // }


    void Update()
    {
        // if (testing)
        // {
        //     if (DetectFaceOnce())
        //     {
        //         Vector3 pos = facePosition * dikali;
        //         Vector3 pos1 = pos + facePositionx;
        //         //kubus.transform.position = pos1;
        //         kubus.transform.position = Vector3.Lerp(kubus.transform.position, pos1, 1f);
        //     }
        // }
    }



    void OnDestroy()
    {
        frameMat?.Dispose();
        grayMat?.Dispose();
        resizedGrayMat?.Dispose();
        faceCascade?.Dispose();
        faces?.Dispose();
    }

    public bool DetectFaceOnce()
    {
        bool foundFace = false;

        if (inputRawImage == null || inputRawImage.texture == null)
            return false;

        Texture srcTex = inputRawImage.texture;

        // 🔹 Copy GPU texture langsung tanpa GetPixels32 / ReadPixels
        if (srcTex is Texture2D t2d)
        {
            if (tempTex.width != t2d.width || tempTex.height != t2d.height)
                tempTex.Reinitialize(t2d.width, t2d.height);

            Graphics.CopyTexture(t2d, tempTex);
        }
        else if (srcTex is RenderTexture rt)
        {
            if (tempTex.width != rt.width || tempTex.height != rt.height)
                tempTex.Reinitialize(rt.width, rt.height);

            RenderTexture tempRT = RenderTexture.GetTemporary(rt.width, rt.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(rt, tempRT);
            RenderTexture.active = tempRT;
            tempTex.ReadPixels(new UnityEngine.Rect(0, 0, rt.width, rt.height), 0, 0);
            tempTex.Apply(false);
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(tempRT);
        }
        else
        {
            Debug.LogWarning("⚠️ Input texture tidak dikenali.");
            return false;
        }

        // 🔹 Convert ke OpenCV Mat
        Utils.texture2DToMat(tempTex, frameMat);
        Imgproc.cvtColor(frameMat, grayMat, Imgproc.COLOR_RGB2GRAY);
        Imgproc.equalizeHist(grayMat, grayMat);

        // 🔹 Resize agar deteksi lebih ringan
        Imgproc.resize(grayMat, resizedGrayMat, new Size(grayMat.cols() / 2, grayMat.rows() / 2));

        // 🔹 Deteksi wajah
        Size minS = new Size(faceMinSize.x / 2, faceMinSize.y / 2);
        Size maxS = (faceMaxSize.x > 0 && faceMaxSize.y > 0)
            ? new Size(faceMaxSize.x / 2, faceMaxSize.y / 2)
            : new Size();

        faceCascade.detectMultiScale(resizedGrayMat, faces, faceScaleFactor, faceMinNeighbors, 0, minS, maxS);
        CvRect[] faceArray = faces.toArray();
        foundFace = faceArray.Length > 0;

        // 🔹 Jika wajah ditemukan → hitung posisi wajah (tengah rect pertama)
        if (foundFace)
        {
            CvRect face = faceArray[0];
            CvRect scaledFace = new CvRect(face.x * 2, face.y * 2, face.width * 2, face.height * 2);

            // Hitung posisi tengah wajah dalam pixel
            float centerX = scaledFace.x + scaledFace.width / 2f;
            float centerY = scaledFace.y + scaledFace.height / 2f;

            // Konversi ke koordinat relatif (-1..1) agar bisa dipakai di Unity scene
            float normalizedX = (centerX / frameMat.width()) * 2f - 1f;
            float normalizedY = (centerY / frameMat.height()) * 2f - 1f;

            facePosition = new Vector3(normalizedX, -normalizedY, 0f); // dibalik Y karena Unity origin di bawah
        }

        // 🔹 Gambar kotak wajah (opsional debug)
        if (drawDebug && foundFace)
        {
            foreach (CvRect face in faceArray)
            {
                CvRect scaledFace = new CvRect(face.x * 2, face.y * 2, face.width * 2, face.height * 2);
                Imgproc.rectangle(frameMat, scaledFace, new Scalar(255, 0, 0, 255), 2);
            }
        }

        // 🔹 Update preview ke UI
        Utils.matToTexture2D(frameMat, tempTex);
        if (preview != null)
            preview.texture = tempTex;

        return foundFace;
    }
}
