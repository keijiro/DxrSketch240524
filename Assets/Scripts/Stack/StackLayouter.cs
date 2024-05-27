using UnityEngine;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Jobs;

namespace Sketch {

[ExecuteInEditMode]
public sealed class StackLayouter : MonoBehaviour, IInstanceLayouter
{
    #region Editable properties

    [field:SerializeField]
    public StackConfig Config = StackConfig.Default();

    #endregion

    #region IInstanceLayouter implementation

    public int InstanceCount => Elements.Length;
    public uint Seed => Config.Seed;

    public JobHandle ScheduleJob(Transform parent, TransformAccessArray xforms)
      => StackXformJob.Schedule(Config, Elements, parent, xforms);

    #endregion

    #region Element array and lazy initializer

    NativeArray<StackElement> Elements
      => _elements.IsCreated ? _elements
           : (_elements = (new StackBuilder(Config)).CreateNativeArray());

    NativeArray<StackElement> _elements;

    #endregion

    #region MonoBehaviour implementation

    void OnValidate()
      => _elements.SafeRelease();

    void OnDestroy()
      => _elements.SafeRelease();

    void OnDisable()
      => _elements.SafeRelease();

    #endregion
}

} // namespace Sketch
