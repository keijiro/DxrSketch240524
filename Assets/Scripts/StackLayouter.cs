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
            if (++level >= rand.RangeXY(Config.Level)) return;

            var h = rand.RangeXY(Config.Height);

            {
                var pos = center + math.float3(0, 0, h * 0.5f);
                var size = math.float3(extent, h);
                buffer.Add(new StackElement(pos, size));
            }

            var ratio1 = rand.NextFloat(0.2f, 0.8f);
            var ratio2a = rand.NextFloat(0.2f, 0.8f);
            var ratio2b = rand.NextFloat(0.2f, 0.8f);

            var origin = center + math.float3(-0.5f * extent, h);

            var ext1 = extent * math.float2(    ratio1,     ratio2a);
            var ext2 = extent * math.float2(    ratio1, 1 - ratio2a);
            var ext3 = extent * math.float2(1 - ratio1,     ratio2b);
            var ext4 = extent * math.float2(1 - ratio1, 1 - ratio2b);

            var shrink = Config.Shrink;

            AddElement(origin + math.float3(         ext1.x * 0.5f,          ext1.y * 0.5f, 0), ext1 * shrink, level);
            AddElement(origin + math.float3(         ext2.x * 0.5f, ext1.y + ext2.y * 0.5f, 0), ext2 * shrink, level);
            AddElement(origin + math.float3(ext1.x + ext3.x * 0.5f,          ext3.y * 0.5f, 0), ext3 * shrink, level);
            AddElement(origin + math.float3(ext1.x + ext4.x * 0.5f, ext3.y + ext4.y * 0.5f, 0), ext4 * shrink, level);
        }

        AddElement(math.float3(0, 0, 0), math.float2(1, 1), 0);

        _elements = new NativeArray<StackElement>(buffer.ToArray(), Allocator.Persistent);
    }

    #endregion
}

} // namespace Sketch
