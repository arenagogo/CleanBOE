using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SetupGameMode : MonoBehaviour
{
    public static SetupGameMode instance;
    public TMP_Dropdown modeOption;
    public TextMeshProUGUI textDescripsiGameMode;
    public Button btnPlay;
    public TextMeshProUGUI gameModeStatustext;

    [SerializeField] private List<DescriptionGameMode> gameMode;
    public HeartRateVisualizer heartRateVisualizer;


    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        btnPlay.onClick.AddListener(SetButtonPlay);
    }

    public void SetButtonPlay()
    {
        if (GlobalVariable.smartWatchConnected == false && GlobalVariable.gamemode == GlobalVariable.GAMEMODE.SMARTWACTH)
        {
            MainMenuSnapBattle.Instance.OpenSettingButton();
            return;
        }

        MainMenuSnapBattle.Instance.OpenAccounMenu(true);
    }

    public void SetGameMode()
    {
        switch (modeOption.value)
        {
            case 0:
                GlobalVariable.gamemode = GlobalVariable.GAMEMODE.FACEMODE;
                textDescripsiGameMode.text = gameMode[0].gameModeDescription;
                gameModeStatustext.text = "FACE MODE";
                break;
            case 1:
                GlobalVariable.gamemode = GlobalVariable.GAMEMODE.SMARTWACTH;
                textDescripsiGameMode.text = gameMode[1].gameModeDescription;
                gameModeStatustext.text = "SMARTWATCH";
                heartRateVisualizer.SetupGameMode();

                if (GlobalVariable.smartWatchConnected == false)
                    MainMenuSnapBattle.Instance.OpenSettingExplan();

                break;
            case 2:
                GlobalVariable.gamemode = GlobalVariable.GAMEMODE.VOICEMODE;
                textDescripsiGameMode.text = gameMode[2].gameModeDescription;
                gameModeStatustext.text = "VOICE MODE";
                break;
            default:
                GlobalVariable.gamemode = GlobalVariable.GAMEMODE.FACEMODE;
                textDescripsiGameMode.text = gameMode[3].gameModeDescription;
                gameModeStatustext.text = "FACE MODE";
                break;
        }
    }


    public Toggle toggleSmartWatch;
    public Toggle toggleFaceMode;
    public Toggle acourntToogle;
    public void SetGameModeFaceMode()
    {
        if (toggleSmartWatch.isOn)
        {
            return;
        }
        modeOption.SetValueWithoutNotify(0);
        GlobalVariable.gamemode = GlobalVariable.GAMEMODE.FACEMODE;
        // textDescripsiGameMode.text = gameMode[0].gameModeDescription;
        gameModeStatustext.text = "FACE MODE";
    }

    public void SetGameModeVoiceMode()
    {
        modeOption.SetValueWithoutNotify(2);
        GlobalVariable.gamemode = GlobalVariable.GAMEMODE.VOICEMODE;
        //  textDescripsiGameMode.text = gameMode[2].gameModeDescription;
        gameModeStatustext.text = "VOICE MODE";
    }

    public void SetGameModeSmartWatch()
    {
        if (toggleFaceMode.isOn)
        {
            return;
        }

        if (!GlobalVariable.smartWatchConnected)
        {
            MainMenuSnapBattle.Instance.OpenSettingExplan();
            acourntToogle.isOn = false;
            // toggleFaceMode.isOn = true;
            // toggleSmartWatch.isOn = false;
            return;
        }

        modeOption.SetValueWithoutNotify(1);
        GlobalVariable.gamemode = GlobalVariable.GAMEMODE.SMARTWACTH;
        // textDescripsiGameMode.text = gameMode[1].gameModeDescription;
        gameModeStatustext.text = "SMARTWATCH";
    }

    public void SetGameModeSmartWatch(bool isConnect)
    {
        if (isConnect)
        {
            modeOption.SetValueWithoutNotify(1);
            GlobalVariable.gamemode = GlobalVariable.GAMEMODE.SMARTWACTH;
            textDescripsiGameMode.text = gameMode[1].gameModeDescription;
            gameModeStatustext.text = "SMARTWATCH";
        }
        else
        {
            modeOption.SetValueWithoutNotify(0);
            // GlobalVariable.gamemode = GlobalVariable.GAMEMODE.SMARTWACTH;
            textDescripsiGameMode.text = gameMode[0].gameModeDescription;
            gameModeStatustext.text = "FACE MODE";
        }

    }

    public void ResetFaceMode()
    {
        if (GlobalVariable.smartWatchConnected == false)
        {
            GlobalVariable.gamemode = GlobalVariable.GAMEMODE.FACEMODE;
            toggleFaceMode.isOn = true;
            toggleSmartWatch.isOn = false;
            modeOption.SetValueWithoutNotify(0);
            gameModeStatustext.text = "FACE MODE";


        }
        else
        {
            if (toggleSmartWatch.isOn)
            {
                modeOption.SetValueWithoutNotify(1);
                GlobalVariable.gamemode = GlobalVariable.GAMEMODE.SMARTWACTH;
                gameModeStatustext.text = "SMARTWATCH";
            }
        }
    }

    [Serializable]
    public class DescriptionGameMode
    {
        public GAMEMODE gamemode;
        public string gameModeDescription;
    }

    [Serializable]
    public enum GAMEMODE
    {
        FACEMODE,
        SMARTWATCH,
        VOICEMODE
    }
}
