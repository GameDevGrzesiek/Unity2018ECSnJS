using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(RocketExplodeSystem))]
public class VFXSystem : ComponentSystem
{
    public static void PlayVFXExplode(Vector3 pos)
    {
        PoolManager.instance.RocketExplosionPool.SpawnObject(pos, Quaternion.identity);
    }

    public struct SpaceShipGroup
    {
        [ReadOnly]
        public ComponentDataArray<RocketDock> RocketDocks;

        [ReadOnly]
        public ComponentDataArray<Position> Positions;

        [ReadOnly]
        public ComponentDataArray<Rotation> Rotations;

        public EntityArray Entities;
        public readonly int Length;
    }

    [Inject] private SpaceShipGroup m_spaceShipGroup;

    private Dictionary<int, SpaceShip> m_spaceShipVFXMap;

    public struct RocketGroup
    {
        [ReadOnly]
        public ComponentDataArray<RocketProximityState> ProximityStates;

        [ReadOnly]
        public ComponentDataArray<Position> Positions;

        [ReadOnly]
        public ComponentDataArray<Rotation> Rotations;

        public EntityArray Entities;
        public readonly int Length;
    }

    [Inject] private RocketGroup m_rocketGroup;

    private Dictionary<int, Rocket> m_rocketVFXMap;

    protected override void OnCreateManager(int capacity)
    {
        base.OnCreateManager(capacity);

        m_spaceShipVFXMap = new Dictionary<int, SpaceShip>();
        m_rocketVFXMap = new Dictionary<int, Rocket>();
    }

    protected override void OnUpdate()
    {
        UpdateSpaceShipVFX();

        if (GameManager.instance.PlayRocketVFX)
            UpdateRocketVFX();
    }

    private void UpdateSpaceShipVFX()
    {
        for (int i = 0; i < m_spaceShipGroup.Length; ++i)
        {
            int index = m_spaceShipGroup.Entities[i].Index;
            Vector3 pos = m_spaceShipGroup.Positions[i].Value;
            Quaternion rot = m_spaceShipGroup.Rotations[i].Value;

            if (!m_spaceShipVFXMap.ContainsKey(index))
                m_spaceShipVFXMap.Add(index, (SpaceShip)PoolManager.instance.SpaceShipPool.SpawnObject(pos, rot));

            m_spaceShipVFXMap[index].transform.position = pos;
            m_spaceShipVFXMap[index].transform.rotation = rot;
        }
    }

    private void UpdateRocketVFX()
    {
        for (int i = 0; i < m_rocketGroup.Length; ++i)
        {
            int index = m_rocketGroup.Entities[i].Index;
            Vector3 pos = m_rocketGroup.Positions[i].Value;
            Quaternion rot = m_rocketGroup.Rotations[i].Value;
            int state = m_rocketGroup.ProximityStates[i].Value;

            if (!m_rocketVFXMap.ContainsKey(index))
                m_rocketVFXMap.Add(index, (Rocket)PoolManager.instance.RocketPool.SpawnObject(pos, rot));

            m_rocketVFXMap[index].transform.position = pos;
            m_rocketVFXMap[index].transform.rotation = rot;
            m_rocketVFXMap[index].SetBurstType(state);
        }
    }

    public void RemoveRocketVFXAtEntity(int index)
    {
        if (!m_rocketVFXMap.ContainsKey(index))
            return;

        PoolManager.instance.RocketPool.ReturnToPool(m_rocketVFXMap[index]);

        m_rocketVFXMap.Remove(index);
    }
}