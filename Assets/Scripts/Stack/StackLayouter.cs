using UnityEngine;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Jobs;

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
      => _elements = StackBuilder.CreateElementArray(Config);

    void OnDestroy()
      => _elements.Dispose();

    #endregion
}

} // namespace Sketch
