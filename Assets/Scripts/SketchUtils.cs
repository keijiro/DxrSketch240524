using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

namespace Sketch {

public static class SketchUtils
{
    public static T GetLiveComponent<T>(this MonoBehaviour self) where T : class
    {
        var c = self.GetComponent<T>();
        return c != null ? c : null;
    }

    public static NativeArray<T> NewTempJobArray<T>(int count) where T : struct
      => new NativeArray<T>(count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

    public static float SNorm(ref this Random rand)
      => rand.NextFloat(-0.5f, 0.5f);

    public static float UNorm(ref this Random rand)
      => rand.NextFloat(0.0f, 1.0f);

    public static float RangeXY(ref this Random rand, float2 minmax)
      => math.lerp(minmax.x, minmax.y, rand.UNorm());
}

} // namespace Sketch
