using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;   // The player
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 0, -10);

    void Start()
    {
        FindPlayerAndSnap();
    }

    private void FindPlayerAndSnap()
    {
        // 1. Try to find the player using your static reference
        if (PlayerReference.PlayerTransform != null)
        {
            target = PlayerReference.PlayerTransform;
        }
        else
        {
            // Fallback: search by name
            GameObject p = GameObject.Find("PLAYER");
            if (p != null) target = p.transform;
        }

        // 2. IMPORTANT: Instant Snap
        // This prevents the camera from "flying" from the scene origin to the player
        if (target != null)
        {
            transform.position = target.position + offset;
        }
    }

    void LateUpdate()
    {
        // If we lost the target (e.g., scene transition), try to find it again
        if (target == null)
        {
            FindPlayerAndSnap();
            return;
        }

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        transform.position = smoothedPosition;
    }
}