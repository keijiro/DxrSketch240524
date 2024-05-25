using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Sketch {

[ExecuteInEditMode]
public sealed class InstancedRenderer : MonoBehaviour, ITimeControl, IPropertyPreview
{
    #region Editable properties

    [field:SerializeField]
    public Mesh[] Meshes { get; set; }

    [field:SerializeField]
    public Material Material { get; set; }

    [field:SerializeField]
    public float Time { get; set; }

    #endregion

    #region Associated objects

    InstancePool _pool;

    IInstanceLayouter LayouterComponent => this.GetLiveComponent<IInstanceLayouter>();

    #endregion

    #region ITimeControl / IPropertyPreview implementation

    public void OnControlTimeStart() {}
    public void OnControlTimeStop() {}
    public void SetTime(double time) => Time = (float)time;
    public void GatherProperties(PlayableDirector dir, IPropertyCollector drv)
      => drv.AddFromName<InstancedRenderer>(gameObject, "<Time>k__BackingField");

    #endregion

    #region MonoBehaviour implementation

    void OnDisable()
    {
        _pool?.Dispose();
        _pool = null;
    }

    void LateUpdate()
    {
        if (_pool == null) _pool = new InstancePool();
        _pool.Capacity = LayouterComponent.InstanceCount;
        _pool.Meshes = Meshes;
        _pool.Material = Material;
        _pool.RandomSeed = LayouterComponent.Seed;
        LayouterComponent.ScheduleJob(_pool.Xforms).Complete();
    }

    #endregion
}

} // namespace Sketch
