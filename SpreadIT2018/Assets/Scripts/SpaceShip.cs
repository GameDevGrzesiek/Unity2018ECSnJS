using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceShip : CustomBehaviour
{
    public Transform RocketDock;
    public Vector3 InitialPos = Vector3.zero;
    private Vector3 m_posOffset = Vector3.zero;

	void Start ()
    {
        Restart();
    }

    public override void Restart()
    {
        base.Restart();

        transform.position = InitialPos;
        transform.rotation = Quaternion.identity;
    }

    public void OnEnable()
    {
        StartCoroutine(CyclicShooting());
        StartCoroutine(CyclicPosChange());
    }

    public void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator CyclicShooting()
    {
        yield return new WaitForSeconds(2);

        while (true)
        {
            var rocket = (Rocket) PoolManager.instance.RocketPool.SpawnObject(RocketDock.position, RocketDock.rotation);
            if (rocket)
                rocket.Shoot();

            yield return new WaitForSeconds(1);
        }
    }

    IEnumerator CyclicPosChange()
    {
        yield return new WaitForSeconds(2);

        while (true)
        {
            m_posOffset = new Vector3(Random.Range(-2.5f, 2.5f), Random.Range(-2.5f, 2.5f), 0);
            yield return new WaitForSeconds(5);
        }
    }

	void Update ()
    {
        float step = GameManager.instance.ObjectSpeed * Time.deltaTime;

        Vector3 dir = Vector3.RotateTowards(transform.forward, GameManager.instance.Comet.position + m_posOffset, step, 0.0f);
        transform.rotation = Quaternion.LookRotation(dir);

        Vector3 pos = Vector3.MoveTowards(transform.position, GameManager.instance.Comet.position + InitialPos, step);

        Vector3 posFlat = new Vector3(pos.x, pos.y, 0);
        Vector3 maxPos = InitialPos + m_posOffset;
        Vector3 flatDir = maxPos - posFlat;
        if (flatDir.magnitude > 0.1)
            pos += flatDir.normalized * Time.deltaTime;

        transform.position = pos;
    }
}
