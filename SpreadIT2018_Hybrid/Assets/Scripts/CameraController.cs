using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    readonly Vector3 m_initialOffset = new Vector3(-100, 0, -100);
    Vector3 m_curPos = Vector3.zero;
    Vector3 m_curRot = Vector3.zero;
    bool m_follow = true;

	void Start ()
    {
        Restart();
	}

    private void Restart()
    {
        m_curPos = GameManager.instance.Comet.position + m_initialOffset;
        this.transform.position = m_curPos;
    }
	
	void Update()
    {
        if (m_follow)
        {
            m_curPos = GameManager.instance.Comet.position + m_initialOffset;
            m_curRot = Vector3.zero;
        }

        this.transform.position = m_curPos;
        this.transform.eulerAngles = m_curRot;
    }

    public void SetToFollow()
    {
        m_follow = !m_follow;
    }

    public void MoveCam(float forward, float right)
    {
        m_follow = false;
        m_curPos += transform.forward * forward + transform.right * right;
    }

    public void RotateCam(float yaw, float pitch)
    {
        m_curRot.x -= pitch;
        m_curRot.y += yaw;
    }
}
