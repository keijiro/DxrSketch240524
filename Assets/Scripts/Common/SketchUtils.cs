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

    public static AffineTransform AffineTransform(in Transform xform)
      => new AffineTransform(xform.position, xform.rotation, xform.localScale);

    public static void SafeRelease<T>(ref this NativeArray<T> array) where T : struct
    {
        if (array.IsCreated) array.Dispose();
    }

    public static float SNorm(ref this Random rand)
      => rand.NextFloat(-0.5f, 0.5f);

    public static float UNorm(ref this Random rand)
      => rand.NextFloat(0.0f, 1.0f);

    public static float2 UNorm2(ref this Random rand)
      => rand.NextFloat2(0.0f, 1.0f);

    public static float RangeXY(ref this Random rand, float2 minmax)
      => rand.NextFloat(minmax.x, minmax.y);

    public static float RangeXYPow(ref this Random rand, float2 minmax, float pow)
      => math.lerp(minmax.x, minmax.y, math.pow(rand.UNorm(), pow));

    public static float RangeXYPow(ref this Random rand, float3 minmaxpow)
      => math.lerp(minmaxpow.x, minmaxpow.y, math.pow(rand.UNorm(), minmaxpow.z));

    public static float2 RangeXYPow2(ref this Random rand, float3 minmaxpow)
      => math.lerp(minmaxpow.x, minmaxpow.y, math.pow(rand.UNorm2(), minmaxpow.z));

    public static int RangeXY(ref this Random rand, int2 minmax)
      => rand.NextInt(minmax.x, minmax.y);
}

} // namespace Sketch
