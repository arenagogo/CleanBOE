using System;
using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class RoomItem : MonoBehaviour
{
    public TextMeshProUGUI nicknameOwner;
    public Image ppOwner;

    public string id;
    public string roomId;
    public string roomName;
    public string title;
    public string host;
    //public List<string> participants;
    public int maxParticipants;
    public string status;

    public void SetDataRoom(string id, string roomId, string roomName, string title, string host, int maxParticipants, string status)
    {
        this.id = id;
        this.roomId = roomId;
        this.roomName = roomName;
        this.title = title;
        this.host = host;
       // this.participants = participants;
        this.maxParticipants = maxParticipants;
        this.status = status;
        nicknameOwner.text = title;
    }

    public void SetPP(Sprite sp)
    {
        ppOwner.sprite = sp;
    }
}
