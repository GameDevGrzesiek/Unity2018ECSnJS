using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class MoveSystem : JobComponentSystem
{
    [BurstCompile]
    struct MoveJob : IJobProcessComponentData<Translation, Rotation, ObjectSpeed>
    {
        [ReadOnly]
        public float3 vec3CometPosition;

        [ReadOnly]
        public float deltaTime;

        public void Execute(ref Translation position, [ReadOnly] ref Rotation rotation, [ReadOnly] ref ObjectSpeed speed)
        {
            float step = speed.Value * deltaTime;
            position.Value = Vector3.MoveTowards(position.Value, vec3CometPosition, step);
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        MoveJob moveJob = new MoveJob
        {
            vec3CometPosition = GameManager.instance.Comet.position,
            deltaTime = Time.deltaTime,
        };

        JobHandle moveHandle = moveJob.Schedule(this, inputDeps);

        return moveHandle;
    }
}
