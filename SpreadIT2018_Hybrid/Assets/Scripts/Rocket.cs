using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public class Rocket : CustomBehaviour
{
    public ParticleSystem FarParticles;
    public ParticleSystem CloseParticles;
    private bool bTriggered = false;

    void Start() {}

    public override void Restart()
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
    }

    public void OnEnable()
    {
        if (GameManager.instance.PlayRocketVFX)
        {
            if (CloseParticles)
                CloseParticles.Stop();

            if (FarParticles)
                FarParticles.Play();
        }
        else
        {
            if (CloseParticles)
            {
                CloseParticles.Stop();
                CloseParticles.gameObject.SetActive(false);
            }

            if (FarParticles)
            {
                FarParticles.Stop();
                FarParticles.gameObject.SetActive(false);
            }
        }

        bTriggered = false;
    }

    public void OnDisable()
    {
        if (GameManager.instance.PlayRocketVFX)
        {
            if (CloseParticles)
                CloseParticles.Stop();

            if (FarParticles)
                FarParticles.Stop();
        }

        bTriggered = false;
    }

    public void OnTriggerEnter(Collider other)
    {
        var explosionObj = PoolManager.instance.RocketExplosionPool.SpawnObject(transform.position, transform.rotation);
        var explosion = explosionObj ? explosionObj.GetComponent<RocketExplosion>() : null;
        if (explosion)
            explosion.Explode();

        bTriggered = true;
    }

    public void UpdateState(bool closeToComet)
    {
        if (closeToComet)
        {
            if (FarParticles && FarParticles.isPlaying)
                FarParticles.Stop();

            if (CloseParticles && !CloseParticles.isPlaying)
                CloseParticles.Play();
        }

        if (bTriggered)
            PoolManager.instance.RocketPool.ReturnToPool(this.gameObject);
    }
}
