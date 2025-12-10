using UnityEngine;
using TMPro;

public class EquipmentInventoryRow : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI bodyPartText;
    public TextMeshProUGUI bluntText;
    public TextMeshProUGUI pierceText;
    public TextMeshProUGUI slashText;

    /// <summary>
    /// Set the row data for this body part
    /// </summary>
    /// <param name="bodyPart">Name of the body part (e.g., "Torso")</param>
    /// <param name="blunt">Total blunt defense</param>
    /// <param name="pierce">Total pierce defense</param>
    /// <param name="slash">Total slash defense</param>
    public void SetData(string bodyPart, float blunt, float pierce, float slash)
    {
        if (bodyPartText != null) bodyPartText.text = bodyPart;
        if (bluntText != null) bluntText.text = blunt.ToString("0");
        if (pierceText != null) pierceText.text = pierce.ToString("0");
        if (slashText != null) slashText.text = slash.ToString("0");
    }
}
