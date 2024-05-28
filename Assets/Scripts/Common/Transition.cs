using Unity.Mathematics;

namespace Sketch {

[System.Serializable]
public struct Transition
{
    public float In;
    public float Stay;
    public float Out;
    public float Jitter;

    public static Transition Default()
      => new Transition() { In = 2, Stay = 1, Out = 2, Jitter = 0.5f };

    public (float, float) FadeInOut(float time, float rand01)
    {
        float Pow(float x)
        {
            x = x * x;
            x = x * x;
            return x * x;
        }
        time -= rand01 * Jitter;
        return (1 - Pow(1 - math.saturate(time / In)),
                Pow(math.saturate((time - In - Stay) / Out)));
    }
}

} // namespace Sketch
