using UnityEngine;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using System.Collections.Generic;
using Random = Unity.Mathematics.Random;

namespace Sketch {

public sealed class StackLayouter : MonoBehaviour, IInstanceLayouter
{
    #region Editable properties

    [field:SerializeField]
    public StackConfig Config = StackConfig.Default();

    #endregion

    #region IInstanceLayouter implementation

    public int InstanceCount => _elements.Length;
    public uint Seed => Config.Seed;

    public JobHandle ScheduleJob(TransformAccessArray xforms)
      => StackXformJob.Schedule(Config, _elements, xforms);

    #endregion

    #region MonoBehaviour implementation

    NativeArray<StackElement> _elements;

    void Start()
    {
        var buffer = new List<StackElement>();

        var rand = new Random(Config.Seed);
        rand.NextUInt();

        void AddElement(float3 center, float2 extent, int level)
        {
            if (++level >= 4) return;

            var h = rand.RangeXY(Config.Height);

            var p = center + math.float3(0, 0, h * 0.5f);
            var s = math.float3(extent, h);
            buffer.Add(new StackElement(p, s));

            var c2 = p + math.float3(0, 0, h * 0.5f);
            var e2 = extent * 0.48f;
            var offs = math.float3(extent, 0) * 0.25f;
            AddElement(c2 + offs * math.float3(-1, -1, 1), e2, level);
            AddElement(c2 + offs * math.float3(+1, -1, 1), e2, level);
            AddElement(c2 + offs * math.float3(-1, +1, 1), e2, level);
            AddElement(c2 + offs * math.float3(+1, +1, 1), e2, level);
        }

        AddElement(math.float3(0, 0, 0), math.float2(1, 1), 0);

        _elements = new NativeArray<StackElement>(buffer.ToArray(), Allocator.Persistent);
    }

    #endregion
}

} // namespace Sketch
