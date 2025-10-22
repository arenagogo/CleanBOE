using System.Collections;

using UnityEngine;
using UnityEngine.UI;

public class AutoScroll : MonoBehaviour
{
    public ScrollRect scrollRect;

    // Panggil ini setiap selesai Instantiate chat bubble baru
    public void ScrollToBottom()
    {
        if (!isActiveAndEnabled) return;
        StartCoroutine(ScrollBottomRoutine());
    }

    IEnumerator ScrollBottomRoutine()
    {
        // 1) Paksa layout selesai
        Canvas.ForceUpdateCanvases();
        if (scrollRect.content)
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);

        // 2) Set ke bawah
        scrollRect.verticalNormalizedPosition = 0f;
        Canvas.ForceUpdateCanvases();

        // 3) Ulangi di frame berikutnya (beberapa layout butuh 1 frame)
        yield return null;
        if (scrollRect.content)
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
        scrollRect.verticalNormalizedPosition = 0f;

        // (opsional) kalau ada scrollbar, samakan nilainya juga
        if (scrollRect.verticalScrollbar)
            scrollRect.verticalScrollbar.value = 0f;
    }
}
