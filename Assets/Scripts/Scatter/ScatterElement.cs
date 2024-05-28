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
    public float Time;

    public static JobHandle Schedule(in ScatterConfig config,
                                     Transform parent,
                                     float time,
                                     TransformAccessArray xforms)
      => new ScatterXformJob()
           { Config = config,
             Parent = SketchUtils.AffineTransform(parent),
             Time = time }.Schedule(xforms);

    static float Pow(float x)
    {
        x = x * x;
        x = x * x;
        return x * x;
    }

    public void Execute(int index, TransformAccess xform)
    {
        var rand = Random.CreateFromIndex((uint)index ^ Config.Seed);
        rand.NextUInt();

        var time = Time - rand.NextFloat();
        var fade_in = 1 - Pow(1 - math.saturate(time));
        var fade_out = Pow(math.saturate(time - 3));

        var pos = (rand.NextFloat3() - 0.5f) * Config.Extent;
        pos.y += Config.Scale.y * (fade_in + fade_out - 1);

        var scale = Config.Scale * (fade_in - fade_out);

        xform.localPosition = math.transform(Parent, pos);
        xform.localRotation = math.rotation(Parent.rs);
        xform.localScale = scale;
    }
}

#endregion

} // namespace Sketch
