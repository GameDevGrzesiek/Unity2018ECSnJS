using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using System;
using UnityEngine;

[Serializable]
public struct ObjectSpeed : IComponentData
{
    public float Value;
}

public class ObjectSpeedComponent : ComponentDataProxy<ObjectSpeed> { }