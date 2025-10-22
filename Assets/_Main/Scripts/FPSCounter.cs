using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    private float deltaTime = 0.0f;
    public float _fps;

    void Update()
    {
        // smoothing pakai exponential moving average
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    void OnGUI()
    {
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(10, 10, w, h * 2 / 100); // posisi pojok kiri atas
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 50; // ukuran font dinamis sesuai layar
        style.normal.textColor = Color.white;

        float fps = 1.0f / deltaTime;
        string text = string.Format("{0:0.} FPS", fps);
        _fps = fps;
        GUI.Label(rect, text, style);
    }
}
