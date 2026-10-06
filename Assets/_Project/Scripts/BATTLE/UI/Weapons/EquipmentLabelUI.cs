using TitusGames.Framework;
using TMPro;
using UnityEngine;

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
        ServiceLocator.Current.TryGet<IEquipmentService>(out var equipmentService);
        if (equipmentService == null) return;

        // Update Main Hand - Check both the instance and the SO
        if (equipmentService.mainHandWeapon != null && equipmentService.mainHandWeapon.itemSO != null)
        {
            mainHandText.text = equipmentService.mainHandWeapon.itemSO.itemName;
        }
        else
        {
            mainHandText.text = "Empty";
        }

        // Update Off Hand - Check both the instance and the SO
        if (equipmentService.offHandShield != null && equipmentService.offHandShield.itemSO != null)
        {
            offHandText.text = equipmentService.offHandShield.itemSO.itemName;
        }
        else
        {
            offHandText.text = "Empty";
        }
    }
}