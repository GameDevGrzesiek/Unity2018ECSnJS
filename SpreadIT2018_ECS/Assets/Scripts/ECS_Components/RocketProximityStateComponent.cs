using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct RocketProximityState : IComponentData
{
    public int Value; // 0 - far, 1 - close, 2 - collision
}

public class RocketProximityStateComponent : ComponentDataWrapper<RocketProximityState> { }
