using UnityEngine.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Sketch {

#region Config struct

[System.Serializable]
public struct StackConfig
{
    public uint Seed;
    public float2 Height;

    public static StackConfig Default()
      => new StackConfig() { Seed = 1, Height = math.float2(0.2f, 0.4f) };
}

#endregion

#region Element struct

public struct StackElement
{
    public float3 Position;
    public float3 Size;

    public StackElement(float3 position, float3 size)
    {
        Position = position;
        Size = size;
    }
}

#endregion

#region Transform update job

[BurstCompile]
public struct StackXformJob : IJobParallelForTransform
{
    public StackConfig Config;
    public NativeArray<StackElement> Elements;

    public static JobHandle Schedule(in StackConfig config,
                                     NativeArray<StackElement> elements,
                                     TransformAccessArray xforms)
      => new StackXformJob()
           { Config = config, Elements = elements }.Schedule(xforms);

    public void Execute(int index, TransformAccess xform)
    {
        var e = Elements[index];
        xform.localPosition = e.Position.xzy;
        xform.localScale = e.Size.xzy;
    }
}

#endregion

} // namespace Sketch
