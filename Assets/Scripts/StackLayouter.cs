using UnityEngine;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using System.Collections.Generic;

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

    void Start()
    {
        var buffer = new List<StackElement>();
        buffer.Add(new StackElement(math.float3(0, 0, 0), math.float3(1, 1, 1)));
        StackSubElement(buffer, buffer[0], 0);
        _elements = new NativeArray<StackElement>(buffer.ToArray(), Allocator.Persistent);
    }

    #endregion

    #region Stack builder

    NativeArray<StackElement> _elements;

    void StackSubElement(List<StackElement> buffer, in StackElement parent, int level)
    {
        if (++level >= 3) return;
        var p = parent.Position + parent.Size * math.float3(0, 0.5f, 0);
        var s = parent.Size * 0.5f;
        var s2 = s * math.float3(0.9f, 1, 0.9f);
        var sub1 = new StackElement(p + s * math.float3(-0.5f, 0.5f, -0.5f), s2);
        var sub2 = new StackElement(p + s * math.float3(+0.5f, 0.5f, -0.5f), s2);
        var sub3 = new StackElement(p + s * math.float3(-0.5f, 0.5f, +0.5f), s2);
        var sub4 = new StackElement(p + s * math.float3(+0.5f, 0.5f, +0.5f), s2);
        buffer.Add(sub1);
        buffer.Add(sub2);
        buffer.Add(sub3);
        buffer.Add(sub4);
        StackSubElement(buffer, sub1, level);
        StackSubElement(buffer, sub2, level);
        StackSubElement(buffer, sub3, level);
        StackSubElement(buffer, sub4, level);
    }

    #endregion
}

} // namespace Sketch
