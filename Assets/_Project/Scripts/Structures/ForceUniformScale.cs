using UnityEngine;

public class ForceUniformScale : MonoBehaviour
{
    void Awake()
    {
        // This forces the world scale to be exactly 1,1,1 
        // regardless of how stretched the house is.
        transform.localScale = Vector3.one;
        Vector3 worldScale = transform.lossyScale;

        transform.localScale = new Vector3(
            1f / worldScale.x,
            1f / worldScale.y,
            1f / worldScale.z
        );
    }
}