using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    public ParticleSystem FarBurst;
    public ParticleSystem CloseBurst;

    private void OnEnable()
    {
        SetBurstType(0);
    }

    private void OnDisable()
    {
        SetBurstType(2);
    }

    public void SetBurstType(int type)
    {
        if (type == 0)
        {
            FarBurst.Play(true);
            CloseBurst.Stop(true);
        }
        else if (type == 1)
        {
            FarBurst.Stop(true);
            CloseBurst.Play(true);
        }
        else
        {
            FarBurst.Stop(true);
            CloseBurst.Stop(true);
        }
    }
}
