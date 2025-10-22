using UnityEngine;
using DG.Tweening;
using TMPro;
public class Loading : MonoBehaviour
{
    public static Loading instance;
    private CanvasGroup _canvasGroup;
    public TextMeshProUGUI errorText;

    private void Awake()
    {
        instance = this;
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void ShowLoading()
    {
        
        _canvasGroup.DOFade(0.5f, 0.2f);
        _canvasGroup.interactable = true;
        _canvasGroup.interactable = true;
    }

    public void HideLoading()
    {
        Invoke(nameof(_HideLoading), 0.5f);
    }

    void _HideLoading()
    {
        errorText.text = string.Empty;
        _canvasGroup.DOFade(0, 0.2f);
        _canvasGroup.interactable = false;
        _canvasGroup.interactable = false;
    }

    public void ShowErrorText(string error)
    {
        errorText.text = error;
    }
}
