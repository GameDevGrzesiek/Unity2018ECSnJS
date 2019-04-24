using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketExplosion : MonoBehaviour
{
    private ParticleSystem particle;

    void Start() { }

    private void OnEnable()
    {
        if(!particle)
            particle = GetComponent<ParticleSystem>();

        if (!particle)
            return;

        if (!GameManager.instance.PlayRocketVFX)
        {
            var emission = particle.emission;
            emission.enabled = false;
            return;
        }

        particle.Play(true);
    }

    public void OnParticleSystemStopped()
    {
        PoolManager.instance.RocketExplosionPool.ReturnToPool(this);
    }
}
