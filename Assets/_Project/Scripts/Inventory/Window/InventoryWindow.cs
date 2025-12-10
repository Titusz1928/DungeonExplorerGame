using UnityEngine;
using TMPro;

public class InventoryWindow : MonoBehaviour
{
    [SerializeField] private Transform rowContainer;

    //Inventory Items Tab
    [SerializeField] private GameObject itemsRowPrefab;
    [SerializeField] private GameObject itemsTitleRowPrefab;

    //Inventory Equipment Tab
    [SerializeField] private GameObject equipmentTitleRowPrefab;
    [SerializeField] private GameObject equipmentRowPrefab;
    [SerializeField] private GameObject equipmentStatRowPrefab;

    [SerializeField] private Transform player;
    [SerializeField] private Inventory inventory;

    [SerializeField] private GameObject moveWindow;
    private ItemInstance pendingMoveItem;

    [SerializeField] private GameObject itemInfoWindow;
    private ItemInstance pendinginfoItem;


    private short tabselected;

    private void Start()
    {
        //Refresh();
    }

    public void Initialize(Inventory inv, Transform playerTransform)
    {
        inventory = inv;
        player = playerTransform;
        tabselected = 0;
        Refresh();
    }

    public void Refresh()
    {
        if (inventory == null)
        {
            Debug.LogError("Inventory not assigned!");
            return;
        }

        switch (tabselected)
        {
            case 0:
                buildItemSection();
                break;
            case 1:
                buildEquipmentSection();
                break;
            case 2:

                break;
        }

    }

    public void itemsTabSelected()
    {
        //destroying other things

        tabselected = 0;
        Refresh();
    }

    public void equipmentTabSelected()
    {
        //destroying other things
        //destroying items
        foreach (Transform child in rowContainer)
            Destroy(child.gameObject);

        tabselected = 1;
        Refresh();
    }

    public void statsnskillsTabSelected()
    {
        //destroying other things
        //destroying items
        foreach (Transform child in rowContainer)
            Destroy(child.gameObject);

        tabselected = 2;
        Refresh();
    }

    public void buildItemSection()
    {
        if (rowContainer == null || itemsRowPrefab == null)
        {
            Debug.LogError("Row container or row prefab not assigned!");
            return;
        }

        foreach (Transform child in rowContainer)
            Destroy(child.gameObject);


        if (itemsTitleRowPrefab != null)
        {
            Instantiate(itemsTitleRowPrefab, rowContainer);
        }
        else
        {
            Debug.LogWarning("titleRowPrefab not assigned! No title row will be created.");
        }

        foreach (ItemInstance instance in inventory.items)
        {
            GameObject rowObj = Instantiate(itemsRowPrefab, rowContainer);

            InventoryRow row = rowObj.GetComponent<InventoryRow>();
            row.SetData(instance, this);
        }
    }


    public void buildEquipmentSection()
    {
        if (rowContainer == null)
        {
            Debug.LogError("Row container not assigned!");
            return;
        }

        // Clear existing rows
        foreach (Transform child in rowContainer)
            Destroy(child.gameObject);

        // Add title row
        if (equipmentTitleRowPrefab != null)
        {
            Instantiate(equipmentTitleRowPrefab, rowContainer);
        }
        else
        {
            Debug.LogWarning("equipmentTitleRowPrefab not assigned! No title row will be created.");
        }

        // Iterate over all body parts (ArmorSlot enum)
        foreach (ArmorSlot slot in System.Enum.GetValues(typeof(ArmorSlot)))
        {
            if (slot == ArmorSlot.None) continue; // skip None

            // Get total defense values from EquipmentManager
            ArmorDefense totalDefense = EquipmentManager.Instance.GetTotalDefenseForSlot(slot);

            // Instantiate a row prefab for this body part
            if (equipmentRowPrefab != null)
            {
                GameObject rowObj = Instantiate(equipmentRowPrefab, rowContainer);
                EquipmentInventoryRow row = rowObj.GetComponent<EquipmentInventoryRow>();
                if (row != null)
                {
                    row.SetData(
                        slot.ToString(),       // body part name
                        totalDefense.blunt,    // total blunt
                        totalDefense.pierce,   // total pierce
                        totalDefense.slash     // total slash
                    );
                }
                else
                {
                    Debug.LogError("EquipmentInventoryRow component not found on prefab!");
                }
            }
            else
            {
                Debug.LogWarning("equipmentRowPrefab not assigned!");
            }
        }


        // --- Sneak row ---
        if (equipmentStatRowPrefab != null)
        {
            GameObject sneakRow = Instantiate(equipmentStatRowPrefab, rowContainer);
            sneakRow.GetComponent<EquipmentStatRow>()
                .SetData("Sneak", EquipmentManager.Instance.GetTotalSneak());
        }

        // --- Conspicuousness row ---
        if (equipmentStatRowPrefab != null)
        {
            GameObject consRow = Instantiate(equipmentStatRowPrefab, rowContainer);
            consRow.GetComponent<EquipmentStatRow>()
                .SetData("Conspicuousness", EquipmentManager.Instance.GetTotalConspicuousness());
        }

    }

    public void OnInfoButtonPressed(ItemInstance item)
    {
        Debug.Log($"Info pressed for {item.itemSO.itemName}");
        pendinginfoItem = item; // store the item if needed later

        // Open the item info window via WindowManager
        GameObject windowGO = WindowManager.Instance.OpenWindow(itemInfoWindow);

        // Get the ItemInfoWindow component from the instantiated window
        ItemInfoWindow infoWindow = windowGO.GetComponent<ItemInfoWindow>();

        if (infoWindow != null)
        {
            infoWindow.Show(item); // <-- pass the ItemInstance here
        }
        else
        {
            Debug.LogError("ItemInfoWindow component not found on the window prefab!");
        }
    }

    public void OnUseButtonPressed(ItemInstance item)
    {
        ItemSO so = item.itemSO;

        // --------------------------
        // BASIC EQUIP / UNEQUIP CHECK
        // --------------------------
        if (item.isEquipped)
        {
            // Already equipped → UNEQUIP
            switch (so)
            {
                case WeaponItemSO weapon:
                    EquipmentManager.Instance.UnequipMainHand(item);
                    MessageManager.Instance.ShowMessageDirectly($"Unequipped {weapon.itemName}");
                    Refresh();
                    return;

                case ShieldItemSO shield:
                    EquipmentManager.Instance.UnequipShield(item);
                    MessageManager.Instance.ShowMessageDirectly($"Unequipped {shield.itemName}");
                    Refresh();
                    return;

                case ArmorItemSO armor:
                    EquipmentManager.Instance.UnequipArmor(item);
                    MessageManager.Instance.ShowMessageDirectly($"Unequipped {armor.itemName}");
                    Refresh();
                    return;
            }
        }

        // -----------------------------------------------------
        // FROM HERE ON, THE ITEM IS NOT EQUIPPED → EQUIP IT
        // -----------------------------------------------------

        switch (so)
        {
            // --------------------------
            // CONSUMABLES
            // --------------------------
            case ConsumableItemSO consumable:
                Debug.Log($"Using consumable: {consumable.itemName}");

                PlayerStateManager.Instance.addHealth(consumable.healthAmount);
                PlayerStateManager.Instance.addStamina(consumable.staminaAmount);

                inventory.RemoveItem(item.itemSO);

                Sprite infoIcon = Resources.Load<Sprite>("UI/Icons/heal");
                MessageManager.Instance.ShowMessageDirectly(
                    $"+{consumable.healthAmount} HP   +{consumable.staminaAmount} STAMINA",
                    infoIcon
                );

                Refresh();
                break;


            // --------------------------
            // WEAPONS
            // --------------------------
            case WeaponItemSO weapon:
                EquipmentManager.Instance.EquipMainHand(item);

                MessageManager.Instance.ShowMessageDirectly(
                    $"Equipped {weapon.itemName}"
                );

                Refresh();
                break;


            // --------------------------
            // SHIELD
            // --------------------------
            case ShieldItemSO shield:
                EquipmentManager.Instance.EquipShield(item);

                MessageManager.Instance.ShowMessageDirectly(
                    $"Equipped {shield.itemName}"
                );

                Refresh();
                break;


            // --------------------------
            // ARMOR
            // --------------------------
            case ArmorItemSO armor:
                EquipmentManager.Instance.EquipArmor(item);

                MessageManager.Instance.ShowMessageDirectly(
                    $"Equipped {armor.itemName}"
                );

                Refresh();
                break;


            // --------------------------
            // DEFAULT
            // --------------------------
            default:
                Debug.Log($"Item cannot be used: {so.itemName}");
                MessageManager.Instance.ShowMessageDirectly($"{so.itemName} cannot be used");
                break;
        }
    }



    public void OnMoveButtonPressed(ItemInstance item)
    {
        Debug.Log($"Move pressed for {item.itemSO.itemName}");

        pendingMoveItem = item;

        // Spawn MoveWindow through WindowManager
        GameObject windowGO = WindowManager.Instance.OpenWindow(moveWindow);

        // Get MoveWindow component from the instantiated window
        MoveWindow moveWindowInstance = windowGO.GetComponent<MoveWindow>();

        // Tell the window which item is being moved
        moveWindowInstance.OpenMoveWindow(item, inventory, player);
    }



    public void OnDropButtonPressed(ItemInstance item)
    {
        if (inventory == null) return;

        // ---------------------------
        // UNEQUIP IF NECESSARY
        // ---------------------------
        if (item.isEquipped)
        {
            switch (item.itemSO)
            {
                case ShieldItemSO shield:
                    EquipmentManager.Instance.UnequipShield(item);
                    break;
                case ArmorItemSO armor:
                    EquipmentManager.Instance.UnequipArmor(item);
                    break;
                case WeaponItemSO weapon:
                    EquipmentManager.Instance.UnequipMainHand(item);
                    break;

            }
        }

        // Remove from inventory
        inventory.RemoveItem(item.itemSO);
        Refresh();

        // Spawn item in the world
        Vector3 dropPos = player.transform.position + player.transform.forward * 1.5f;
        ItemSpawner.Instance.SpawnWorldItem(item.itemSO, dropPos, 1);
    }
}

