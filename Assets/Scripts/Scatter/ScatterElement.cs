using UnityEngine;
using UnityEngine.Jobs;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

namespace Sketch {

#region Config struct

[System.Serializable]
public struct ScatterConfig
{
    public int InstanceCount;
    public float3 Extent;
    public float3 Scale;
    public uint Seed;

    public static ScatterConfig Default()
      => new ScatterConfig()
           { InstanceCount = 10,
             Extent = math.float3(1, 1, 1),
             Scale = 0.1f,
             Seed = 1 };
}

#endregion

#region Transform update job

[BurstCompile]
public struct ScatterXformJob : IJobParallelForTransform
{
    public ScatterConfig Config;
    public AffineTransform Parent;

    public static JobHandle Schedule(in ScatterConfig config,
                                     Transform parent,
                                     TransformAccessArray xforms)
      => new ScatterXformJob()
           { Config = config,
             Parent = SketchUtils.AffineTransform(parent) }.Schedule(xforms);

    public void Execute(int index, TransformAccess xform)
    {
        var rand = Random.CreateFromIndex((uint)index + Config.Seed);
        rand.NextUInt();
        var p = (rand.NextFloat3() - 0.5f) * Config.Extent;
        xform.localPosition = math.transform(Parent, p);
        xform.localRotation = math.rotation(Parent.rs);
        xform.localScale = Config.Scale;
    }
}

#endregion

} // namespace Sketch
