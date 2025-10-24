using UnityEngine;

[ExecuteAlways] // biar kelihatan juga saat di Editor (tanpa play)
[RequireComponent(typeof(RectTransform))]
public class GetScreenSize : MonoBehaviour
{
    private Canvas canvas;
    private RectTransform rectTransform;

    [Header("Screen Info (Pixel)")]
    [SerializeField] private float screenWidth;
    [SerializeField] private float screenHeight;

    [Header("Canvas Info (UI Units)")]
    [SerializeField] private float canvasWidth;
    [SerializeField] private float canvasHeight;

    public float dikalikanWidth = 1f;

    public RectTransform[] rectT;

    private void Update()
    {
        // Ambil ukuran layar (pixel)
        screenWidth = Screen.width;
        screenHeight = Screen.height;

        // Ambil ukuran canvas (UI unit)
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        if (canvas != null)
        {
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            canvasWidth = canvasRect.rect.width;
            canvasHeight = canvasRect.rect.height;

            float akak = canvasHeight / dikalikanWidth;

            foreach (var rt in rectT)
            {
                if (rt != null)
                {
                    rt.localScale = new Vector2(akak, akak);
                }
            }
        }
    }
}
