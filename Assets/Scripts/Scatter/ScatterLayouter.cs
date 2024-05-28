using UnityEngine;
using UnityEngine.Jobs;
using Unity.Jobs;

namespace Sketch {

[ExecuteInEditMode]
public sealed class ScatterLayouter : MonoBehaviour, IInstanceLayouter
{
    #region Editable properties

    [field:SerializeField]
    public ScatterConfig Config = ScatterConfig.Default();

    #endregion

    #region IInstanceLayouter implementation

    public int InstanceCount => Config.InstanceCount;
    public uint Seed => Config.Seed;

    public JobHandle ScheduleJob
      (Transform parent, float time, TransformAccessArray xforms)
      => ScatterXformJob.Schedule(Config, parent, time, xforms);

    #endregion
}

} // namespace Sketch
