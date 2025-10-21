using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class MainMenuSelecting : MonoBehaviour
{
    public static MainMenuSelecting Instance;
    public TMP_Dropdown dropdownSelectDevice;
    public TMP_InputField inputIdSmartWach;
    public Button buttonConnect, buttonExplanation;
    public string hypeRateID;
    public TextMeshProUGUI statusDevice;
    public GameObject panelSmartwatch, panelFace;
  //  public GameObject logoArenago;
    public CanvasGroup panelMainMenu;



    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
       // GlobalVariable.smartWatchConnected = false;
    }

    public void ShowButtonConnect()
    {
        if(inputIdSmartWach.text.Length > 0)
        {
            buttonConnect.interactable = true;
        }
        else
        {
            buttonConnect.interactable = false;
        }
        hypeRateID = inputIdSmartWach.text;
    }

    public void SelectedDevice()
    {
        if(dropdownSelectDevice.value == 0)
        {
            panelSmartwatch.SetActive(true);
            panelFace.SetActive(false);
           // GlobalVariable.smartWatchConnected = true;
        }
        else
        {
            panelSmartwatch.SetActive(false);
            panelFace.SetActive(true);
           // GlobalVariable.smartWatchConnected = false;
        }
    }


    //public void PlayGame()
    //{
    //    logoArenago.transform.DOLocalMoveY(0, 1f).SetEase(Ease.InBack).OnComplete(()=> { 
        
    //        logoArenago.transform.DOScale(logoArenago.transform.localScale + new Vector3(0.3f, 0.3f, 0.3f), 1.5f).SetEase(Ease.InBack).OnComplete(()=>
    //        {
    //            panelMainMenu.DOFade(0, 0.5f);
    //            panelMainMenu.interactable = false;
    //            panelMainMenu.blocksRaycasts = false;
    //        });

    //    });
    //}
}
