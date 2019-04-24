using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(SpaceShipMoveSystem))]
public class RocketMoveSystem : JobComponentSystem
{
    public static readonly int cometRaycastMask = 1 << LayerMask.NameToLayer("Comet");
    public static readonly float raycastLength = 10.0f;

    EntityManager m_entityManager;

    private NativeArray<RaycastHit> m_raycastHits;
    private NativeArray<RaycastCommand> m_raycastCommands;
    private JobHandle m_raycastJH;

    public struct RocketProximityGroup
    {
        public ComponentDataArray<RocketProximityState> ProximityStates;

        public EntityArray Entities;

        [ReadOnly]
        public ComponentDataArray<Translation> Positions;

        [ReadOnly]
        public ComponentDataArray<Rotation> Rotations;

        [ReadOnly]
        public ComponentDataArray<RocketCollision> Collisions;

        public readonly int Length;
    }

    [Inject] private RocketProximityGroup m_rocketProximityGroup;

    [BurstCompile]
    struct RocketRotateJob : IJobProcessComponentData<Rotation, Translation, RocketCollision>
    {
        [ReadOnly]
        public float3 vec3CometPosition;

        public void Execute([WriteOnly] ref Rotation rotation, [ReadOnly] ref Translation position, [ReadOnly] ref RocketCollision rocketCollision)
        {
            Vector3 dir = vec3CometPosition - position.Value;
            if (dir != Vector3.zero)
            {
                var rot = Quaternion.LookRotation(dir.normalized).eulerAngles;
                rot.x += 90f;
                rotation.Value = Quaternion.Euler(rot);
            }
        }
    }

    protected override void OnCreateManager()
    {
        base.OnCreateManager();

        m_entityManager = World.Active.GetOrCreateManager<EntityManager>();
        m_raycastCommands = new NativeArray<RaycastCommand>(0, Allocator.Persistent);
        m_raycastHits = new NativeArray<RaycastHit>(0, Allocator.Persistent);
    }

    protected override void OnDestroyManager()
    {
        base.OnDestroyManager();

        m_raycastCommands.Dispose();
        m_raycastHits.Dispose();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        RocketRotateJob rocketRotateJob = new RocketRotateJob
        {
            vec3CometPosition = GameManager.instance.Comet.position,
        };

        JobHandle rotateHandle = rocketRotateJob.Schedule(this, inputDeps);

        if (m_raycastJH.IsCompleted)
        {
            World.Active.GetOrCreateManager<SpaceShipMoveSystem>().m_spaceShipOffsetMoveJH.Complete();

            for (int i = 0; i < m_rocketProximityGroup.Length; ++i)
            {
                float DistBetweenCenters = Vector3.Distance(GameManager.instance.Comet.position, m_rocketProximityGroup.Positions[i].Value);
                if (DistBetweenCenters < GameManager.instance.CometColliderRadius * 2 + m_rocketProximityGroup.Collisions[i].Height)
                {
                    m_entityManager.SetComponentData(m_rocketProximityGroup.Entities[i], new RocketProximityState { Value = 2 });
                }
                else if (i < m_raycastHits.Length - 1 && m_raycastHits[i].collider != null)
                {
                    m_entityManager.SetComponentData(m_rocketProximityGroup.Entities[i], new RocketProximityState { Value = 1 });
                }
                else
                {
                    m_entityManager.SetComponentData(m_rocketProximityGroup.Entities[i], new RocketProximityState { Value = 0 });
                }
            }

            m_raycastHits.Dispose();
            m_raycastCommands.Dispose();

            int opSize = m_rocketProximityGroup.Length;

            m_raycastCommands = new NativeArray<RaycastCommand>(opSize, Allocator.Persistent);
            m_raycastHits = new NativeArray<RaycastHit>(opSize, Allocator.Persistent);

            for (int i = 0; i < opSize; ++i)
            {
                Vector3 pos = m_rocketProximityGroup.Positions[i].Value;
                Vector3 dir = GameManager.instance.Comet.position - pos;
                m_raycastCommands[i] = new RaycastCommand(pos, dir.normalized, raycastLength, cometRaycastMask);
            }

            m_raycastJH = RaycastCommand.ScheduleBatch(m_raycastCommands, m_raycastHits, GameManager.SubJobsSplit, rotateHandle);
        }

        return JobHandle.CombineDependencies(rotateHandle, m_raycastJH);
    }
}