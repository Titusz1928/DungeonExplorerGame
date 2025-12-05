using UnityEngine;

using UnityEngine;

public class Bar : MonoBehaviour
{
    [SerializeField] private Transform fill;

    public void SetValue(float normalizedValue)
    {
        fill.localScale = new Vector3(normalizedValue, 1f, 1f);
    }
}
