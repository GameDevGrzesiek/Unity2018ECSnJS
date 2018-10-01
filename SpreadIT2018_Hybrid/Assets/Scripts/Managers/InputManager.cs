using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : Singleton<InputManager>
{
    public enum InputMode
    {
        UIMode = 0,
        GameMode
    };

    public InputMode SteeringMode = InputMode.GameMode;
    public CameraController Camera;
    public float CamSpeed = 3f;

	void Start ()
    {
        UIManager.instance.SetInputModeInfo(SteeringMode);
    }
	
	void Update ()
    {
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            SteeringMode = (SteeringMode == InputMode.UIMode) ? InputMode.GameMode : InputMode.UIMode;
            UIManager.instance.SetInputModeInfo(SteeringMode);
        }

        if (Input.GetKeyUp(KeyCode.Space))
            UIManager.instance.ShowHideUI();

        switch (SteeringMode)
        {
            case InputMode.GameMode:
            {
                    float forwardValue = 0f;
                    float rightValue = 0f;

                    float yaw = CamSpeed * Input.GetAxis("Mouse X");
                    float pitch = CamSpeed * Input.GetAxis("Mouse Y");

                    if (Input.GetKey(KeyCode.W))
                        forwardValue += CamSpeed;
                    else if (Input.GetKey(KeyCode.S))
                        forwardValue -= CamSpeed;

                    if (Input.GetKey(KeyCode.D))
                        rightValue += CamSpeed;
                    else if (Input.GetKey(KeyCode.A))
                        rightValue -= CamSpeed;

                    if (Input.GetKeyUp(KeyCode.F) && Camera)
                    {
                        Camera.SetToFollow();
                    }
                    else if (Camera)
                    {
                        Camera.MoveCam(forwardValue, rightValue);
                        Camera.RotateCam(yaw, pitch);
                    }
                }
                break;

            case InputMode.UIMode:
            {

            }break;
        }       
	}
}
