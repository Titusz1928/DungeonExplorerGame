using UnityEngine;

public class NoiseEvent
{
    public Vector3 Position { get; }
    public float BaseStrength { get; }
    public float Lifetime { get; }
    public GameObject Source { get; }

    public NoiseEvent(Vector3 position, float baseStrength, float lifetime, GameObject source)
    {
        Position = position;
        BaseStrength = baseStrength;
        Lifetime = lifetime;
        Source = source;
    }
}
