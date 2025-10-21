using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
public class ChanelRoomManager : MonoBehaviour
{
    public static ChanelRoomManager Instance;
    public TextMeshProUGUI localPlayerNameText;
    public TextMeshProUGUI remotePlayerNameText;
    public CanvasGroup waitingforotherplayers;


    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        
    }
    void Update()
    {
        
    }
}
