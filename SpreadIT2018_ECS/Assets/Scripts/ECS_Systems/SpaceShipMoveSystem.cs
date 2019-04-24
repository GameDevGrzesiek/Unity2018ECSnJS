using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(MoveSystem))]
public class SpaceShipMoveSystem : JobComponentSystem
{
    //public static readonly System.Random RNG = new System.Random();

    private float m_offsetChangeTimer = 0.0f;
    public JobHandle m_offsetChangeJH;
    public JobHandle m_spaceShipOffsetMoveJH;

    [BurstCompile]
    struct SpaceShipOffsetChangeJob : IJobProcessComponentData<MoveOffset>
    {
        public float randValX;
        public float randValY;

        public void Execute([WriteOnly] ref MoveOffset offset)
        {
            float x = randValX;
            x = (x * 5) - 2.5f;

            float y = randValY;
            y = (y * 5) - 2.5f;

            offset.Value = new float3(x, y, 0);
        }
    }

    [BurstCompile]
    struct SpaceShipOffsetMoveJob : IJobProcessComponentData<Translation, InitialPos, MoveOffset>
    {
        [ReadOnly]
        public float deltaTime;

        public void Execute(ref Translation position, [ReadOnly] ref InitialPos initialPos, [ReadOnly] ref MoveOffset moveOffset)
        {
            Vector3 curPos = position.Value;
            Vector3 posFlat = new Vector3(position.Value.x, position.Value.y, 0);
            Vector3 maxPos = initialPos.Value + moveOffset.Value;
            Vector3 flatDir = maxPos - posFlat;
            if (flatDir.magnitude > 0.1)
                curPos += flatDir.normalized * deltaTime;

            position.Value = curPos;
        }
    }

    [BurstCompile]
    struct RocketDockUpdateJob : IJobProcessComponentData<RocketDock, Translation, Rotation>
    {
        public void Execute([WriteOnly] ref RocketDock rocketDock, [ReadOnly] ref Translation position, [ReadOnly] ref Rotation rotation)
        {
            Quaternion curOrientation = rotation.Value;
            float3 rocketDockOffset = curOrientation * new Vector3(0, -1.5f, 1);
            rocketDock.Value = position.Value + rocketDockOffset;
        }
    }

    [BurstCompile]
    struct SpaceShipRotateJob : IJobProcessComponentData<Rotation, Translation, RocketDock>
    {
        [ReadOnly]
        public float3 vec3CometPosition;

        public void Execute([WriteOnly] ref Rotation rotation, [ReadOnly] ref Translation position, [ReadOnly] ref RocketDock rocketDock)
        {
            Vector3 dir = vec3CometPosition - position.Value;
            if (dir != Vector3.zero)
            {
                var rot = Quaternion.LookRotation(dir.normalized).eulerAngles;
                rot.y += 180f;
                rotation.Value = Quaternion.Euler(rot);
            }
        }
    }

    protected override void OnStartRunning()
    {
        base.OnStartRunning();

        m_offsetChangeTimer = 0.0f;
    }

    protected override void OnStopRunning()
    {
        base.OnStopRunning();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        m_offsetChangeTimer += Time.deltaTime;

        JobHandle initialDeps = JobHandle.CombineDependencies(inputDeps, m_offsetChangeJH);

        SpaceShipOffsetMoveJob offsetMoveJob = new SpaceShipOffsetMoveJob
        {
            deltaTime = Time.deltaTime
        };

        m_spaceShipOffsetMoveJH = offsetMoveJob.Schedule(this, JobHandle.CombineDependencies(initialDeps, m_offsetChangeJH));

        if (m_offsetChangeTimer > 5.0f)
        {
            m_offsetChangeTimer = 0.0f;

            SpaceShipOffsetChangeJob offsetChangeJob = new SpaceShipOffsetChangeJob()
            {
                randValX = UnityEngine.Random.Range(-2.5f, 2.5f),
                randValY = UnityEngine.Random.Range(-2.5f, 2.5f)
            };
            m_offsetChangeJH = offsetChangeJob.Schedule(this, m_spaceShipOffsetMoveJH);
        }

        SpaceShipRotateJob spaceShipRotateJob = new SpaceShipRotateJob
        {
            vec3CometPosition = GameManager.instance.Comet.position
        };

        JobHandle spaceShipRotateHandle = spaceShipRotateJob.Schedule(this, m_spaceShipOffsetMoveJH);

        RocketDockUpdateJob rocketDockUpdate = new RocketDockUpdateJob();

        JobHandle returnDeps = rocketDockUpdate.Schedule(this, JobHandle.CombineDependencies(m_spaceShipOffsetMoveJH, m_offsetChangeJH, spaceShipRotateHandle));

        return returnDeps;
    }
}
