using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(SpaceShipMoveSystem))]
public class ShootingSystem : ComponentSystem
{
    public static RenderMesh RocketRenderData;
    public static EntityArchetype RocketArchetype;

    EntityManager m_entityManager;

    private float m_shootingTime = 0f;

    public struct RocketDockGroup
    {
        [ReadOnly]
        public ComponentDataArray<RocketDock> RocketDocks;
        public EntityArray Entities;
        public readonly int Length;
    }

    [Inject] private RocketDockGroup m_rocketDockGroup;

    public struct RocketCollisionGroup
    {
        [ReadOnly]
        public ComponentDataArray<RocketCollision> RocketCollisions;
        [ReadOnly]
        public ComponentDataArray<RocketProximityState> RocketStates;

        public EntityArray Entities;
        public readonly int Length;
    }

    [Inject] private RocketCollisionGroup m_rocketCollisions;

    protected override void OnCreateManager()
    {
        base.OnCreateManager();

        m_entityManager = World.Active.GetOrCreateManager<EntityManager>();

        RocketRenderData = GetLookFromPrototype("RocketPrototype");
        RocketArchetype = m_entityManager.CreateArchetype(typeof(Translation), typeof(Rotation), typeof(ObjectSpeed), 
                                                          typeof(RocketProximityState), typeof(RocketCollision));
    }

    protected override void OnStartRunning()
    {
        base.OnStartRunning();

        m_shootingTime = 0f;
    }

    protected override void OnUpdate()
    {
        m_shootingTime += Time.deltaTime;

        if (RocketRenderData.mesh == null)
        {
            RocketRenderData = GetLookFromPrototype("RocketPrototype");
            return;
        }

        if (m_shootingTime > 1.0f)
        {
            GameManager.instance.RocketCnt = m_rocketCollisions.Length;
            UIManager.instance.RefreshPoolCount();

            if (m_rocketCollisions.Length < m_rocketDockGroup.Length * 4)
            {
                int diff = m_rocketDockGroup.Length * 4 - m_rocketCollisions.Length;

                for (int i = 0; i < m_rocketDockGroup.Length; ++i)
                {
                    SpawnRocket(i);
                    --diff;

                    if (diff == 0)
                        break;
                }
            }

            m_shootingTime = 0;

            /*
                m_shootingTime = 0;
                for (int i = 0; i < m_rocketDockGroup.Length; ++i)
                    SpawnRocket(i);
            */

            List<int> entitiesToReuse = new List<int>();
            for (int i = 0; i < m_rocketCollisions.Length; ++i)
            {
                if (m_rocketCollisions.RocketStates[i].Value == 2)
                    entitiesToReuse.Add(i);
            }

            if (entitiesToReuse.Count > 0)
            {
                for (int i = 0; i < m_rocketDockGroup.Length; ++i)
                {
                    ResetRocket(entitiesToReuse[entitiesToReuse.Count - 1], i);
                    entitiesToReuse.RemoveAt(entitiesToReuse.Count - 1);

                    if (entitiesToReuse.Count == 0)
                        break;
                }
            }
        }
    }

    private void SpawnRocket(int i)
    {
        PostUpdateCommands.CreateEntity(RocketArchetype);
        PostUpdateCommands.SetComponent(new Translation { Value = new float3(m_rocketDockGroup.RocketDocks[i].Value) });
        PostUpdateCommands.SetComponent(new Rotation { Value = quaternion.identity });
        PostUpdateCommands.SetComponent(new ObjectSpeed { Value = GameManager.instance.RocketSpeed });
        PostUpdateCommands.SetComponent(new RocketProximityState { Value = 0 });
        PostUpdateCommands.SetComponent(new RocketCollision { Height = 1.5f, Radius = 0.2f });
        PostUpdateCommands.AddSharedComponent<RenderMesh>(RocketRenderData);
    }

    private void ResetRocket(int rocketID, int rocketDockID)
    {
        m_entityManager.SetComponentData(m_rocketCollisions.Entities[rocketID], new Translation { Value = new float3(m_rocketDockGroup.RocketDocks[rocketDockID].Value) });
        m_entityManager.SetComponentData(m_rocketCollisions.Entities[rocketID], new Rotation { Value = quaternion.identity });
        m_entityManager.SetComponentData(m_rocketCollisions.Entities[rocketID], new RocketProximityState { Value = 0 });
    }

    private static RenderMesh GetLookFromPrototype(string protoName)
    {
        var proto = GameObject.Find(protoName);
        if (!proto)
            return new RenderMesh();

        var result = proto.GetComponent<RenderMeshProxy>().Value;
        Object.Destroy(proto);
        return result;
    }
}
