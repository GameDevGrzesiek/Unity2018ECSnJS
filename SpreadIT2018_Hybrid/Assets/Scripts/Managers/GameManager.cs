using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [SerializeField]
    public bool PlayRocketVFX = true;

    [SerializeField]
    public float ObjectSpeed = 1.0f;

    [SerializeField]
    public float RocketSpeed = 15.0f;

    [SerializeField]
    public int SpaceShipSpread = 10;

    public Transform Comet;

    private float deltaTime = 0.0f;

    void Start ()
    {
        StartCoroutine(InitialSpawn());
    }

	void Update ()
    {
        RenderSettings.skybox.SetFloat("_Rotation", -Time.time * ObjectSpeed);

        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        float msec = deltaTime * 1000.0f;
        UIManager.instance.SetFPSInfo(fps, msec);
    }

    IEnumerator InitialSpawn()
    {
        yield return new WaitUntil(() => (PoolManager.instance.SpaceShipPool.m_cnt > 0));
        SpawnAndPlaceSpaceships();
    }

    public void AddSpaceships()
    {
        PoolManager.instance.SpaceShipPool.Expand(8);
        SpawnAndPlaceSpaceships();

        PoolManager.instance.RocketPool.Expand(32);
        JobManager.instance.AddRockets(32);

        if (PlayRocketVFX)
            PoolManager.instance.RocketExplosionPool.Expand(32);
    }

    public void RemoveSpaceships()
    {
        if (PoolManager.instance.SpaceShipPool.m_cnt <= 1)
            return;

        PoolManager.instance.SpaceShipPool.Expand(-8);
        JobManager.instance.RemoveSpaceShipData(8);

        PoolManager.instance.RocketPool.Expand(-32);

        if (PlayRocketVFX)
            PoolManager.instance.RocketExplosionPool.Expand(-32);
    }

    public void AddRockets()
    {
        PoolManager.instance.RocketPool.Expand(10);
        JobManager.instance.AddRockets(10);

        if (PlayRocketVFX)
            PoolManager.instance.RocketExplosionPool.Expand(10);
    }

    public void RemoveRockets()
    {
        if (PoolManager.instance.RocketPool.m_cnt <= 0)
            return;

        PoolManager.instance.RocketPool.Expand(-10);
        JobManager.instance.RemoveRockets(10);

        if (PlayRocketVFX)
            PoolManager.instance.RocketExplosionPool.Expand(-10);
    }

    public void AddTailStrips()
    {
    }

    public void RemoveTailStrips()
    {
    }

    public void SpawnAndPlaceSpaceships()
    {
        if (PoolManager.instance.SpaceShipPool.m_cnt == 0)
            return;

        if (PoolManager.instance.SpaceShipPool.m_cnt == 1)
        {
            SpawnSpaceShipWithInitialPos(Vector3.zero);
        }
        else
        {
            int sizeToSpread = PoolManager.instance.SpaceShipPool.m_cnt - 1;
            int indexOfPlacement = sizeToSpread / 8;

            // place only last 8 ships
            SpawnSpaceShipWithInitialPos(new Vector3(-SpaceShipSpread * indexOfPlacement, -SpaceShipSpread * indexOfPlacement, 0));
            SpawnSpaceShipWithInitialPos(new Vector3(0, -SpaceShipSpread * indexOfPlacement, 0));
            SpawnSpaceShipWithInitialPos(new Vector3(SpaceShipSpread * indexOfPlacement, -SpaceShipSpread * indexOfPlacement, 0));
            SpawnSpaceShipWithInitialPos(new Vector3(-SpaceShipSpread * indexOfPlacement, 0, 0));
            SpawnSpaceShipWithInitialPos(new Vector3(SpaceShipSpread * indexOfPlacement, 0, 0));
            SpawnSpaceShipWithInitialPos(new Vector3(-SpaceShipSpread * indexOfPlacement, SpaceShipSpread * indexOfPlacement, 0));
            SpawnSpaceShipWithInitialPos(new Vector3(0, SpaceShipSpread * indexOfPlacement, 0));
            SpawnSpaceShipWithInitialPos(new Vector3(SpaceShipSpread * indexOfPlacement, SpaceShipSpread * indexOfPlacement, 0));
        }
    }

    private void SpawnSpaceShipWithInitialPos(Vector3 initialPos)
    {
        var spaceShipObj = PoolManager.instance.SpaceShipPool.SpawnObject(initialPos, Quaternion.identity);
        JobManager.instance.AddSpaceShipData(spaceShipObj, initialPos);
    }
}