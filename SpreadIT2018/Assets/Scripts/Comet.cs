using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Comet : CustomBehaviour
{
    readonly Vector3 m_initialPos = new Vector3(0, 0, 100);

    void Start ()
    {
        Restart();
	}

    public override void Restart()
    {
        base.Restart();

        this.transform.position = m_initialPos;
        this.transform.rotation = Quaternion.identity;
    }
	
	void Update ()
    {
        this.transform.position += this.transform.forward * GameManager.instance.ObjectSpeed * Time.deltaTime;
    }
}
