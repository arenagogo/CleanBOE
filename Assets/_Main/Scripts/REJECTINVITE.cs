using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class REJECTINVITE : MonoBehaviour
{
    public static REJECTINVITE Instance;
    public TextMeshProUGUI username;
    private CanvasGroup cvs;

    private void Awake()
    {
        Instance = this;
        cvs = GetComponent<CanvasGroup>();
    }
    public void SetRejectOpen(string _username)
    {
        cvs.alpha = 1f;
        cvs.interactable = true;
        cvs.blocksRaycasts = true;
        username.text = _username;
    }

    public void HideReject()
    {
        cvs.alpha = 0f;
        cvs.interactable = false;
        cvs.blocksRaycasts = false;
    }
}
