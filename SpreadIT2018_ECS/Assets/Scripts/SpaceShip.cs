using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceShip : MonoBehaviour
{
    public ParticleSystem LeftMain;
    public ParticleSystem RightMain;
    public ParticleSystem LeftAux;
    public ParticleSystem RightAux;

    private void OnEnable()
    {
        LeftMain.Play(true);
        RightMain.Play(true);
        LeftAux.Play(true);
        RightAux.Play(true);
    }

    private void OnDisable()
    {
        LeftMain.Stop(true);
        RightMain.Stop(true);
        LeftAux.Stop(true);
        RightAux.Stop(true);
    }
}
