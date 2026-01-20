using UnityEngine;
using TMPro; // Required for TextMeshPro

public class CoordinateDisplay : MonoBehaviour
{
    private TextMeshProUGUI textElement;

    void Awake()
    {
        textElement = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        // Check if the static reference exists to avoid NullReferenceErrors
        if (GameBoot.PersistentPlayer != null)
        {
            Vector3 pos = GameBoot.PersistentPlayer.transform.position;

            // Formatting to 0 decimal places for a clean look
            textElement.text = $"X: {pos.x:F0}, Y: {pos.y:F0}";
        }
    }
}