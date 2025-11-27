using UnityEngine;

public class InventoryTest : MonoBehaviour
{
    public Inventory inventory;
    public ItemSO testItem;

    void Start()
    {
        inventory.AddItem(testItem, 1);
        inventory.Save();
        inventory.Load();
    }
}
