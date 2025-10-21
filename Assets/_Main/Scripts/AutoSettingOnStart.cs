using UnityEngine;

public class AutoSettingOnStart : MonoBehaviour
{
    public GameObject[] hideGameObject;
    public GameObject[] showGameObject;

    public CanvasGroup[] hideCanvasGroup;
    public CanvasGroup[] showCanvasGroup;




    private void Awake()
    {
      // SetReset();
    }


    [ContextMenu("SetDefault")]
    public void SetReset()
    {
        foreach (GameObject go in hideGameObject)
        {
            go.SetActive(false);
        }

        foreach (GameObject go in showGameObject)
        {
            go.SetActive(true);
        }

        foreach (CanvasGroup cvs in hideCanvasGroup)
        {
            cvs.alpha = 0;
            cvs.interactable = false;
            cvs.blocksRaycasts = false;
        }

        foreach (CanvasGroup cvs in showCanvasGroup)
        {
            cvs.alpha = 1;
            cvs.interactable = true;
            cvs.blocksRaycasts = true;
        }
    }
}
