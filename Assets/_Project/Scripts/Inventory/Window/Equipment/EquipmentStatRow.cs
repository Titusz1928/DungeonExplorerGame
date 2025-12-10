using UnityEngine;
using TMPro;

public class EquipmentStatRow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI labelText;
    [SerializeField] private TextMeshProUGUI valueText;

    public void SetData(string label, float value)
    {
        if (labelText != null) labelText.text = label;
        if (valueText != null) valueText.text = value.ToString("0.##"); // nice formatting
    }
}
