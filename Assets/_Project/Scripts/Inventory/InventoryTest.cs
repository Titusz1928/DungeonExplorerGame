using UnityEngine;

public class InventoryTest : MonoBehaviour
{
    public Inventory inventory;
    public ItemSO testItem1;
    public ItemSO testItem2;
    public ItemSO testItem3;

    void Start()
    {
        inventory.AddItem(testItem1, 1);
        inventory.AddItem(testItem2, 1);
        inventory.AddItem(testItem3, 3);
        inventory.AddItem(testItem1, 1);
        inventory.AddItem(testItem2, 1);
        inventory.Save();
        inventory.Load();
    }
}
