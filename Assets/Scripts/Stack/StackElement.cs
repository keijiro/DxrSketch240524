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
    public int3 Subdivide;
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
             Subdivide = math.int3(1, 8, 4),
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

public class StackBuilder
{
    // Constructor
    public StackBuilder(in StackConfig config)
    {
        _config = config;

        _rand = new Random(config.Seed);
        _rand.NextUInt();

        AddSubelementUniform((float3)0, math.float2(3, 3), 0, config.Grid);
    }

    // Native array generator
    public NativeArray<StackElement> CreateNativeArray()
      => new NativeArray<StackElement>(_buffer.ToArray(), Allocator.Persistent);

    // Configuration
    StackConfig _config;
    Random _rand;

    // Build buffer
    List<StackElement> _buffer = new List<StackElement>();

    // Recursive add-element method
    void AddElement(float3 pos, float2 ext, int level)
    {
        // Extent based cutoff
        if (math.min(ext.x, ext.y) < _config.Cutoff) return;

        // Random height
        var h = _rand.RangeXYPow(_config.Height);

        // Element addition with decimation
        if (_rand.UNorm() >= _config.Decimate)
        {
            var p = pos + math.float3(0, 0, h / 2);
            var s = math.float3(ext, h);
            _buffer.Add(new StackElement(p, s));
        }

        // Random termination
        if (++level >= _rand.RangeXY(_config.Level)) return;

        // Lift up
        pos.z += h;

        // Uniform / non-uniform split
        if (_rand.NextFloat() < _config.Uniformity)
        {
            var div = (int2)_rand.RangeXYPow2(_config.Subdivide);
            AddSubelementUniform(pos, ext, level, div);
        }
        else
        {
            AddSubelementNonUniform(pos, ext, level);
        }
    }

    // Subelement (uniform, nxn)
    void AddSubelementUniform(float3 pos, float2 ext, int level, int2 div)
    {
        pos.xy -= ext / 2;
        ext /= div;

        var size = ext - _rand.RangeXYPow(_config.Margin);

        for (var i = 0; i < div.x; i++)
        {
            for (var j = 0; j < div.y; j++)
            {
                var offs = ext * (math.float2(i, j) + 0.5f);
                AddElement(math.float3(pos.xy + offs, pos.z), size, level);
            }
        }
    }

    // Subelement (non-uniform, 2x2)
    void AddSubelementNonUniform(float3 pos, float2 ext, int level)
    {
        pos.xy -= ext / 2;

        var r1  = _rand.NextFloat(0.2f, 0.8f);
        var r2a = _rand.NextFloat(0.2f, 0.8f);
        var r2b = _rand.NextFloat(0.2f, 0.8f);

        var s1 = ext * math.float2(    r1,     r2a);
        var s2 = ext * math.float2(    r1, 1 - r2a);
        var s3 = ext * math.float2(1 - r1,     r2b);
        var s4 = ext * math.float2(1 - r1, 1 - r2b);

        var m = _rand.RangeXYPow(_config.Margin);

        AddElement(pos + math.float3(       s1.x / 2,        s1.y / 2, 0), s1 - m, level);
        AddElement(pos + math.float3(       s2.x / 2, s1.y + s2.y / 2, 0), s2 - m, level);
        AddElement(pos + math.float3(s1.x + s3.x / 2,        s3.y / 2, 0), s3 - m, level);
        AddElement(pos + math.float3(s1.x + s4.x / 2, s3.y + s4.y / 2, 0), s4 - m, level);
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
