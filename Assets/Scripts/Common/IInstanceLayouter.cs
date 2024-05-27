using UnityEngine;
using UnityEngine.Jobs;
using Unity.Jobs;

namespace Sketch {

public interface IInstanceLayouter
{
    public int InstanceCount { get; }
    public uint Seed { get; }
    public JobHandle ScheduleJob(Transform parent, TransformAccessArray xforms);
}

} // namespace Sketch
