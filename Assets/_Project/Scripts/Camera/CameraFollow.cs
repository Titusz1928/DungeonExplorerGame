using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;   // The player
    public float smoothSpeed = 0.125f;
    public Vector3 offset;     // (0,0,-10) is recommended for 2D

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        transform.position = smoothedPosition;
    }
}
