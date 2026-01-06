using UnityEngine;
using TMPro;

public class EquipmentLabelUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI mainHandText;
    [SerializeField] private TextMeshProUGUI offHandText;

    private void OnEnable()
    {
        RefreshLabels();
    }

    public void RefreshLabels()
    {
        EquipmentManager em = EquipmentManager.Instance;
        if (em == null) return;

        // Update Main Hand - Check both the instance and the SO
        if (em.mainHandWeapon != null && em.mainHandWeapon.itemSO != null)
        {
            mainHandText.text = em.mainHandWeapon.itemSO.itemName;
        }
        else
        {
            mainHandText.text = "Empty";
        }

        // Update Off Hand - Check both the instance and the SO
        if (em.offHandShield != null && em.offHandShield.itemSO != null)
        {
            offHandText.text = em.offHandShield.itemSO.itemName;
        }
        else
        {
            offHandText.text = "Empty";
        }
    }
}