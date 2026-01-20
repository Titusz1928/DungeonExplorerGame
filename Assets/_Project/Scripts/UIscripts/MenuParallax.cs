using UnityEngine;

public class MenuParallax : MonoBehaviour
{
    [Header("Zoom Settings")]
    public bool enableZoom = true;
    public float zoomSpeed = 0.05f;
    public float maxZoom = 1.15f;

    [Header("Drift Settings")]
    public bool enableDrift = true;
    public float driftAmount = 20f;
    public float driftSpeed = 0.5f;

    private Vector3 initialScale;
    private Vector3 initialPosition;
    private float timer;

    void Start()
    {
        initialScale = transform.localScale;
        initialPosition = transform.localPosition;
    }

    void Update()
    {
        timer += Time.deltaTime;

        // 1. Slow Pulsing Zoom
        if (enableZoom)
        {
            // Uses Sin wave to zoom in and out very slowly
            float zoomDelta = Mathf.PingPong(timer * zoomSpeed, maxZoom - 1f);
            transform.localScale = initialScale + new Vector3(zoomDelta, zoomDelta, 0);
        }

        // 2. Slight Drifting Movement
        if (enableDrift)
        {
            // Creates a smooth 'floating' effect
            float x = Mathf.Sin(timer * driftSpeed) * driftAmount;
            float y = Mathf.Cos(timer * driftSpeed * 0.7f) * driftAmount;
            transform.localPosition = initialPosition + new Vector3(x, y, 0);
        }
    }
}