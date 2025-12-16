using UnityEngine;

public class PlayerReference : MonoBehaviour
{
    public static Transform PlayerTransform { get; private set; }

    void Awake()
    {
        if (PlayerTransform != null)
        {
            Destroy(gameObject);
            return;
        }

        PlayerTransform = transform;
    }

    void OnDestroy()
    {
        if (PlayerTransform == transform)
            PlayerTransform = null;
    }
}
