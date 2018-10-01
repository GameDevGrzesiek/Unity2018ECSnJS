using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    public Canvas MainUI;
    public Text FPSInfo;
    public Text InputMode;

    public Text SpaceShipCntInfo;
    public Text RocketCntInfo;
    public Text CometTailCntInfo;

    public Button AddRockets;
    public Button RemoveRockets;

    public Button AddSpaceShips;
    public Button RemoveSpaceShips;

    public Button AddTailStrips;
    public Button RemoveTailStrips;

    void Start ()
    {
        if (AddRockets)
            AddRockets.onClick.AddListener(GameManager.instance.AddRockets);

        if (RemoveRockets)
            RemoveRockets.onClick.AddListener(GameManager.instance.RemoveRockets);

        if (AddSpaceShips)
            AddSpaceShips.onClick.AddListener(GameManager.instance.AddSpaceships);

        if (RemoveSpaceShips)
            RemoveSpaceShips.onClick.AddListener(GameManager.instance.RemoveSpaceships);

        if (AddTailStrips)
            AddTailStrips.onClick.AddListener(GameManager.instance.AddTailStrips);

        if (RemoveTailStrips)
            RemoveTailStrips.onClick.AddListener(GameManager.instance.RemoveTailStrips);

        TempHide();
    }

    void TempHide()
    {
        if (AddTailStrips)
            AddTailStrips.enabled = false;

        if (RemoveTailStrips)
            RemoveTailStrips.enabled = false;

        if (CometTailCntInfo)
            CometTailCntInfo.enabled = false;

        if (AddRockets)
            AddRockets.enabled = false;

        if (RemoveRockets)
            RemoveRockets.enabled = false;
    }
	
	void Update ()
    {
		
	}

    public void ShowHideUI()
    {
        if (!MainUI)
            return;

        MainUI.enabled = !MainUI.enabled;
    }

    public void SetFPSInfo(float fps, float ms)
    {
        if (!FPSInfo)
            return;

        FPSInfo.text = "FPS: " + fps.ToString("0.00") + " , " + ms.ToString("0.0000") + " ms";
    }

    public void SetInputModeInfo(InputManager.InputMode i_mode)
    {
        if (!InputMode)
            return;

        InputMode.text = "[TAB] : Current Mode: " + ((i_mode == InputManager.InputMode.GameMode) ? "Game Mode" : "UI Mode");
    }

    internal void RefreshPoolCount()
    {
        StartCoroutine(DelayedPoolInfoUpdate());
    }

    IEnumerator DelayedPoolInfoUpdate()
    {
        yield return new WaitUntil(() => (SpaceShipCntInfo && RocketCntInfo && CometTailCntInfo));

        SpaceShipCntInfo.text = GameManager.instance.m_spaceShips.Count.ToString() + " Space ships";
        RocketCntInfo.text = GameManager.instance.RocketCnt.ToString() + " Rockets";
    }
}
