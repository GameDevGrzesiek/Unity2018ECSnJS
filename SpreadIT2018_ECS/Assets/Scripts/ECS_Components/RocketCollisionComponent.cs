using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct RocketCollision : IComponentData
{
    public float Radius;
    public float Height;
}

public class RocketCollisionComponent : ComponentDataWrapper<RocketCollision> { }
