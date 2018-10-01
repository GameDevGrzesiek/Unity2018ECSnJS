using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public class Rocket : CustomBehaviour
{
    private bool m_shot = false;
    float m_rotationStep = 0f;

    public ParticleSystem FarParticles;
    public ParticleSystem CloseParticles;

    void Start() {}

    public override void Restart()
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        m_shot = false;
        m_rotationStep = 0;
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
    }

    public void Shoot()
    {
        m_shot = true;
    }

    public void OnTriggerEnter(Collider other)
    {
        var explosion = (RocketExplosion)PoolManager.instance.RocketExplosionPool.SpawnObject(transform.position, transform.rotation);
        if (explosion)
            explosion.Explode();

        PoolManager.instance.RocketPool.ReturnToPool(this);
    }

    void Update ()
    {
		if (m_shot)
        {
            float step = GameManager.instance.RocketSpeed * Time.deltaTime;
            m_rotationStep += step * 5;

            Vector3 dir = Vector3.RotateTowards(transform.forward, GameManager.instance.Comet.position, step, 0.0f);
            transform.rotation = Quaternion.LookRotation(dir);

            transform.position = Vector3.MoveTowards(transform.position, GameManager.instance.Comet.position, step);

            int mask = 1 << LayerMask.NameToLayer("Comet");
            if (Physics.Raycast(transform.position, dir, 15.0f, mask, QueryTriggerInteraction.Collide))
            //if (Vector3.Distance(transform.position, GameManager.instance.Comet.position) < 60.0f)
            {
                Vector3 eulers = transform.rotation.eulerAngles;
                transform.rotation = Quaternion.Euler(new Vector3(eulers.x, eulers.y, eulers.z + m_rotationStep));

                if (GameManager.instance.PlayRocketVFX)
                {
                    if (FarParticles && FarParticles.isPlaying)
                        FarParticles.Stop();

                    if (CloseParticles && !CloseParticles.isPlaying)
                        CloseParticles.Play();
                }
            }
        }
	}
}
