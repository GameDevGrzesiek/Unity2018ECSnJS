using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(RocketMoveSystem))]
public class RocketExplodeSystem : ComponentSystem
{
    EntityManager m_entityManager;
    VFXSystem m_vfxSystem;

    public struct RocketStateGroup
    {
        [ReadOnly]
        public ComponentDataArray<RocketProximityState> ProximityStates;

        [ReadOnly]
        public ComponentDataArray<Translation> Positions;

        public EntityArray Entities;
        public readonly int Length;
    }

    [Inject] private RocketStateGroup m_rocketStateGroup;

    protected override void OnCreateManager()
    {
        base.OnCreateManager();

        m_entityManager = World.Active.GetOrCreateManager<EntityManager>();
        m_vfxSystem = World.Active.GetOrCreateManager<VFXSystem>();
    }

    protected override void OnUpdate()
    {
        for (int i = 0; i < m_rocketStateGroup.Length; ++i)
        {
            if(m_rocketStateGroup.ProximityStates[i].Value == 2)
                DestroyRocket(i);
        }
    }

    private void DestroyRocket(int i)
    {
        if (GameManager.instance.PlayRocketVFX)
            VFXSystem.PlayVFXExplode(m_rocketStateGroup.Positions[i].Value);

        m_vfxSystem.RemoveRocketVFXAtEntity(i);
    }
}