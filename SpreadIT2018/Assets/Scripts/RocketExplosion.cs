using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketExplosion : CustomBehaviour
{
    private ParticleSystem particle;

	void Start ()
    {
        Restart();
    }

    public override void Restart()
    {
        base.Restart();

        if (!particle)
            particle = GetComponent<ParticleSystem>();

        if (!particle)
            return;
    }

    public void Explode()
    {
        if (!particle || !isActiveAndEnabled)
            return;

        particle.Play(true);
    }

    public void OnParticleSystemStopped()
    {
        PoolManager.instance.RocketExplosionPool.ReturnToPool(this);
    }
}
