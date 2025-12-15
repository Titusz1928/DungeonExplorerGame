using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class ItemStatRow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI slotsText;



    public void SetData(Inventory inv, InventoryWindow window)
    {

        slotsText.text = inv.items.Count.ToString()+"/"+inv.maxSlots.ToString();

    }
}
