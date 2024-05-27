using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Serialization;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using System.Collections.Generic;
using Random = Unity.Mathematics.Random;

namespace Sketch {

#region Config struct

[System.Serializable]
public struct StackConfig
{
    public uint Seed;
    public int2 Level;
    public float3 Height;
    public float3 Margin;
    public float Cutoff;
    public float Decimate;
    public float Uniformity;
    public int2 Grid;

    public static StackConfig Default()
      => new StackConfig()
           { Seed = 1,
             Level = math.int2(4, 8),
             Height = math.float3(0.2f, 0.4f, 2),
             Margin = math.float3(0.001f, 0.002f, 2),
             Cutoff = 0.01f,
             Decimate = 0,
             Uniformity = 0.5f,
             Grid = math.int2(1, 1) };
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

#region Builder class

public static class StackBuilder
{
    public static NativeArray<StackElement> CreateElementArray(StackConfig config)
    {
        var buffer = new List<StackElement>();

        var rand = new Random(config.Seed);
        rand.NextUInt();

        void AddElement(float3 center, float2 extent, int level)
        {
            if (math.min(extent.x, extent.y) < config.Cutoff) return;

            var h = rand.RangeXYPow(config.Height);

            if (rand.UNorm() >= config.Decimate)
            {
                var pos = center + math.float3(0, 0, h * 0.5f);
                var size = math.float3(extent, h);
                buffer.Add(new StackElement(pos, size));
            }

            if (++level >= rand.RangeXY(config.Level)) return;

            center.z += h;

            if (rand.NextFloat() < config.Uniformity)
            {
                var div = math.int2((int)math.lerp(1, 8, math.pow(rand.NextFloat(), 4)),
                                    (int)math.lerp(1, 8, math.pow(rand.NextFloat(), 4)));
                AddPropSubElement(center, extent, level, div);
            }
            else
            {
                AddUnpropSubElement(center, extent, level);
            }
        }

        void AddPropSubElement(float3 center, float2 extent, int level, int2 div)
        {
            var origin = center + math.float3(-0.5f * extent, 0);
            var ext = extent / div - rand.RangeXYPow(config.Margin);

            for (var i = 0; i < div.x; i++)
            {
                for (var j = 0; j < div.y; j++)
                {
                    AddElement(math.float3(origin.xy + ext * (math.float2(i, j) + 0.5f), origin.z), ext, level);
                }
            }
        }

        void AddUnpropSubElement(float3 center, float2 extent, int level)
        {
            var ratio1 = rand.NextFloat(0.2f, 0.8f);
            var ratio2a = rand.NextFloat(0.2f, 0.8f);
            var ratio2b = rand.NextFloat(0.2f, 0.8f);

            var origin = center + math.float3(-0.5f * extent, 0);

            var ext1 = extent * math.float2(    ratio1,     ratio2a);
            var ext2 = extent * math.float2(    ratio1, 1 - ratio2a);
            var ext3 = extent * math.float2(1 - ratio1,     ratio2b);
            var ext4 = extent * math.float2(1 - ratio1, 1 - ratio2b);

            var shrink = rand.RangeXYPow(config.Margin);

            AddElement(origin + math.float3(         ext1.x * 0.5f,          ext1.y * 0.5f, 0), ext1 - shrink, level);
            AddElement(origin + math.float3(         ext2.x * 0.5f, ext1.y + ext2.y * 0.5f, 0), ext2 - shrink, level);
            AddElement(origin + math.float3(ext1.x + ext3.x * 0.5f,          ext3.y * 0.5f, 0), ext3 - shrink, level);
            AddElement(origin + math.float3(ext1.x + ext4.x * 0.5f, ext3.y + ext4.y * 0.5f, 0), ext4 - shrink, level);
        }

        //AddElement(math.float3(0, 0, 0), math.float2(3, 3), 0);
        AddPropSubElement((float3)0, math.float2(3, 3), 0, config.Grid);

        return new NativeArray<StackElement>(buffer.ToArray(), Allocator.Persistent);
    }
}

#endregion

#region Transform update job

[BurstCompile]
public struct StackXformJob : IJobParallelForTransform
{
    public StackConfig Config;
    public AffineTransform Parent;
    public NativeArray<StackElement> Elements;

    public static JobHandle Schedule(in StackConfig config,
                                     NativeArray<StackElement> elements,
                                     Transform parent,
                                     TransformAccessArray xforms)
      => new StackXformJob()
           { Config = config,
             Elements = elements,
             Parent = SketchUtils.AffineTransform(parent) }.Schedule(xforms);

    public void Execute(int index, TransformAccess xform)
    {
        var e = Elements[index];
        var scale = math.length(math.mul(Parent, math.float4(1, 0, 0, 0)));
        xform.localPosition = math.transform(Parent, e.Position.xzy);
        xform.localRotation = math.rotation(Parent.rs);
        xform.localScale = e.Size.xzy;
    }
}

#endregion

} // namespace Sketch
