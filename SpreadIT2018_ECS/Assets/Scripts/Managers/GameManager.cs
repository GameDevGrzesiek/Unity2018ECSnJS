using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    private const float m_defaultScale = 0.5f;
    public static readonly int SubJobsSplit = 16;

    [SerializeField]
    public bool PlayRocketVFX = true;

    [SerializeField]
    public float ObjectSpeed = 1.0f;

    [SerializeField]
    public float RocketSpeed = 15.0f;

    [SerializeField]
    public int SpaceShipSpread = 10;

    public Transform Comet;

    public GameObject SpaceShipECSPrefab;
    public GameObject RocketECSPrefab;

    private float deltaTime = 0.0f;
    public EntityManager EManager;
    public List<Entity> m_spaceShips;

    internal int RocketCnt;
    internal float CometColliderRadius = 0;

    void Start()
    {
        m_spaceShips = new List<Entity>();
        EManager = World.Active.GetOrCreateManager<EntityManager>();
        AddAndSpawnSpaceships(1);

        CometColliderRadius = Comet.GetComponent<SphereCollider>().radius;
    }

    void Update()
    {
        RenderSettings.skybox.SetFloat("_Rotation", -Time.time * ObjectSpeed);

        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        float msec = deltaTime * 1000.0f;
        UIManager.instance.SetFPSInfo(fps, msec);
    }

    public void AddSpaceships()
    {
        AddAndSpawnSpaceships(8);
    }

    public void RemoveSpaceships()
    {
        if (m_spaceShips.Count <= 1)
            return;

        AddAndSpawnSpaceships(-8);
    }

    public void AddRockets()
    {
    }

    public void RemoveRockets()
    {
    }

    public void AddTailStrips()
    {
    }

    public void RemoveTailStrips()
    {
    }

    public void AddAndSpawnSpaceships(int cnt)
    {
        List<Entity> newSpaceShipList = new List<Entity>();

        if (cnt > 0)
        {
            for (int i = 0; i < cnt; ++i)
                newSpaceShipList.Add(EManager.Instantiate(SpaceShipECSPrefab));

            m_spaceShips.AddRange(newSpaceShipList);

            if (m_spaceShips.Count == 1)
            {
                SetComponentDataForSpaceShip(0, float3.zero);
            }
            else
            {
                int indexOfPlacement = (m_spaceShips.Count - 1) / 8;
                SetComponentDataForSpaceShip(m_spaceShips.Count - 8, new float3(-SpaceShipSpread * indexOfPlacement, -SpaceShipSpread * indexOfPlacement, 0));
                SetComponentDataForSpaceShip(m_spaceShips.Count - 7, new float3(0, -SpaceShipSpread * indexOfPlacement, 0));
                SetComponentDataForSpaceShip(m_spaceShips.Count - 6, new float3(SpaceShipSpread * indexOfPlacement, -SpaceShipSpread * indexOfPlacement, 0));
                SetComponentDataForSpaceShip(m_spaceShips.Count - 5, new float3(-SpaceShipSpread * indexOfPlacement, 0, 0));
                SetComponentDataForSpaceShip(m_spaceShips.Count - 4, new float3(SpaceShipSpread * indexOfPlacement, 0, 0));
                SetComponentDataForSpaceShip(m_spaceShips.Count - 3, new float3(-SpaceShipSpread * indexOfPlacement, SpaceShipSpread * indexOfPlacement, 0));
                SetComponentDataForSpaceShip(m_spaceShips.Count - 2, new float3(0, SpaceShipSpread * indexOfPlacement, 0));
                SetComponentDataForSpaceShip(m_spaceShips.Count - 1, new float3(SpaceShipSpread * indexOfPlacement, SpaceShipSpread * indexOfPlacement, 0));
            }
        }
        else
        {
            int absCnt = Math.Abs(cnt);

            for (int i = 0; i < absCnt; ++i)
            {
                EManager.DestroyEntity(m_spaceShips[m_spaceShips.Count - 1]);
                m_spaceShips.RemoveAt(m_spaceShips.Count - 1);
            }
        }

        UIManager.instance.RefreshPoolCount();
    }

    private void SetComponentDataForSpaceShip(int index, float3 placement)
    {
        EManager.SetComponentData(m_spaceShips[index], new Translation { Value = new float3(placement) });
        EManager.SetComponentData(m_spaceShips[index], new Rotation { Value = Quaternion.AngleAxis(180, Vector3.up) });
        EManager.SetComponentData(m_spaceShips[index], new NonUniformScale { Value = new float3(0.5f, 0.5f, 0.5f) });
        EManager.SetComponentData(m_spaceShips[index], new ObjectSpeed { Value = GameManager.instance.ObjectSpeed });
        EManager.SetComponentData(m_spaceShips[index], new MoveOffset { Value = new float3(0, 0, 0) });
        EManager.SetComponentData(m_spaceShips[index], new InitialPos { Value = new float3(placement) });
    }
}
