using UnityEngine;
using TMPro;

public class InventoryWindow : MonoBehaviour
{
    [SerializeField] private Transform rowContainer;

    //Inventory Items Tab
    [SerializeField] private GameObject itemsRowPrefab;
    [SerializeField] private GameObject itemsTitleRowPrefab;
    [SerializeField] private GameObject itemsStatsRowPrefab;
    [SerializeField] private GameObject moveWindow;
    private ItemInstance pendingMoveItem;
    [SerializeField] private GameObject itemInfoWindow;
    private ItemInstance pendinginfoItem;
    [SerializeField] private GameObject dropWindow;
    private ItemInstance pendingDropItem;
    [SerializeField] private GameObject ripClothingWindowPrefab;

    //Inventory Equipment Tab
    [SerializeField] private GameObject equipmentTitleRowPrefab;
    [SerializeField] private GameObject equipmentRowPrefab;
    [SerializeField] private GameObject equipmentStatRowPrefab;

    //Inventory Injuruies & Skills Tab
    [SerializeField] private GameObject injurySectionPrefab;
    [SerializeField] private GameObject skillsRowPrefab;

    [SerializeField] private Transform player;
    [SerializeField] private Inventory inventory;

    

    


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
                buildHealthNSkillsSection();
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


        if (itemsStatsRowPrefab != null)
        {
            GameObject rowObj = Instantiate(itemsStatsRowPrefab, rowContainer);
            ItemStatRow row = rowObj.GetComponent<ItemStatRow>();
            row.SetData(inventory, this);
        }
        else
        {
            Debug.LogWarning("itemsStatsRowPrefab not assigned! No item stat row will be created.");
        }

        //if (itemsTitleRowPrefab != null)
        //{
        //    Instantiate(itemsTitleRowPrefab, rowContainer);
        //}
        //else
        //{
        //    Debug.LogWarning("titleRowPrefab not assigned! No title row will be created.");
        //}

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


    public void buildHealthNSkillsSection()
    {
        if (rowContainer == null)
        {
            Debug.LogError("Row container not assigned!");
            return;
        }

        foreach (Transform child in rowContainer)
            Destroy(child.gameObject);

        // 1. Get the Player and InjuryManager
        if (PlayerStateManager.Instance != null)
        {
            GameObject playerObj = PlayerStateManager.Instance.gameObject;
            InjuryManager injuryManager = playerObj.GetComponent<InjuryManager>();

            // 2. Only create the dropdown if the prefab exists AND there is at least one injury
            if (injurySectionPrefab != null && injuryManager != null && injuryManager.activeInjuries.Count > 0)
            {
                GameObject healthObj = Instantiate(injurySectionPrefab, rowContainer);

                InjuryDropdownUI dropdown = healthObj.GetComponent<InjuryDropdownUI>();
                if (dropdown != null)
                {
                    dropdown.Initialize(playerObj);
                }
            }
        }

        foreach (PlayerSkill skill in System.Enum.GetValues(typeof(PlayerSkill)))
        {
            GameObject rowObj = Instantiate(skillsRowPrefab, rowContainer);
            InventorySkillRow row = rowObj.GetComponent<InventorySkillRow>();
            row.SetData(skill);
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
                // Only allow Food or Potion types here
                if (consumable.consumableType == ConsumableType.Food || consumable.consumableType == ConsumableType.Potion)
                {
                    Debug.Log($"Using consumable: {consumable.itemName}");

                    PlayerStateManager.Instance.addHealth(consumable.healthAmount);
                    PlayerStateManager.Instance.addStamina(consumable.staminaAmount);

                    inventory.RemoveItem(item);

                    Sprite infoIcon = Resources.Load<Sprite>("UI/Icons/heal");
                    MessageManager.Instance.ShowMessageDirectly(
                        $"+{consumable.healthAmount} HP   +{consumable.staminaAmount} STAMINA",
                        infoIcon
                    );

                    Refresh();
                }
                else if (consumable.consumableType == ConsumableType.FirstAid)
                {
                    InjuryManager injuryManager = PlayerStateManager.Instance.GetComponent<InjuryManager>();
                    Injury target = injuryManager.GetFirstTreatableInjury();

                    if (target != null)
                    {
                        Debug.Log($"Quick-using First Aid on: {target.bodyPart}");

                        // Apply the bandage using your existing function
                        injuryManager.ApplyBandage(target);

                        // Remove item from inventory
                        inventory.RemoveItem(item);

                        // Notify Player
                        Sprite infoIcon = Resources.Load<Sprite>("UI/Icons/heal"); // Ensure this path exists
                        MessageManager.Instance.ShowMessageDirectly(
                            $"Bandaged {target.bodyPart} ({target.type})",
                            infoIcon
                        );

                        Refresh();
                    }
                    else
                    {
                        // Optional: Tell the player they don't need a bandage
                        MessageManager.Instance.ShowMessageDirectly("No injuries require bandaging.", null);
                        Debug.Log("FirstAid clicked but no treatable injuries found.");
                    }
                }
                break;


            // --------------------------
            // WEAPONS
            // --------------------------
            case WeaponItemSO weapon:
                if (item.currentDurability <= 0)
                {
                    MessageManager.Instance.ShowMessageDirectly($"{weapon.itemName} is broken and cannot be equipped!");
                    return;
                }

                EquipmentManager.Instance.EquipMainHand(item);
                MessageManager.Instance.ShowMessageDirectly($"Equipped {weapon.itemName}");
                Refresh();
                break;

            // --------------------------
            // SHIELD
            // --------------------------
            case ShieldItemSO shield:
                if (item.currentDurability <= 0)
                {
                    MessageManager.Instance.ShowMessageDirectly($"{shield.itemName} is destroyed!");
                    return;
                }

                EquipmentManager.Instance.EquipShield(item);
                MessageManager.Instance.ShowMessageDirectly($"Equipped {shield.itemName}");
                Refresh();
                break;

            // --------------------------
            // ARMOR
            // --------------------------
            case ArmorItemSO armor:
                if (item.currentDurability <= 0)
                {
                    MessageManager.Instance.ShowMessageDirectly($"{armor.itemName} is too damaged to wear!");
                    return;
                }

                EquipmentManager.Instance.EquipArmor(item);
                MessageManager.Instance.ShowMessageDirectly($"Equipped {armor.itemName}");
                Refresh();
                break;

            // --------------------------
            // SCISSORS (ID 19)
            // --------------------------
            case ItemSO genericItem when genericItem.ID == 19:
                if (item.currentDurability <= 0)
                {
                    MessageManager.Instance.ShowMessageDirectly("These scissors are broken and cannot cut anything.");
                    return;
                }

                Debug.Log($"Using special item: {genericItem.itemName}");
                OnRipButtonPressed(item);
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

    public void OnRipButtonPressed(ItemInstance item)
    {
        Debug.Log($"Rip Clothing pressed using {item.itemSO.itemName}");

        // 1. Spawn the Window through WindowManager
        GameObject windowGO = WindowManager.Instance.OpenWindow(ripClothingWindowPrefab);

        // 2. Get the specific RipClothingWindow component
        RipClothingWindow ripWindowInstance = windowGO.GetComponent<RipClothingWindow>();

        // 3. Initialize the window with the scissors and the current inventory
        if (ripWindowInstance != null)
        {
            ripWindowInstance.OpenRipWindow(item, inventory);
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



    public void OnDropButtonPressed(ItemInstance item, bool showWindow)
    {
        if (showWindow && item.quantity > 1)
        {
            // 1) Logic to open your DropWindow prefab (similar to MoveWindow)
            // Pass 'item' to it so it can set the slider max
            Debug.Log("Opening Drop Quantity Window...");

            pendingDropItem = item;

            // Spawn MoveWindow through WindowManager
            GameObject windowGO = WindowManager.Instance.OpenWindow(dropWindow);

            // Get MoveWindow component from the instantiated window
            DropWindow dropWindowInstance = windowGO.GetComponent<DropWindow>();

            // Tell the window which item is being moved
            dropWindowInstance.OpenDropWindow(item, inventory, player);
        }
        else
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

            // 1. Determine spawn position
            Vector3 spawnPos = PlayerReference.PlayerTransform.position;

            // 2. CREATE A TEMPORARY INSTANCE FOR THE GROUND
            // We create a "copy" that only represents the 1 item being dropped.
            ItemInstance singleDrop = new ItemInstance(item.itemSO, 1);
            singleDrop.currentDurability = item.currentDurability;

            // 3. Spawn using the temporary "single" instance
            ItemSpawner.Instance.SpawnWorldItem(singleDrop, spawnPos);

            // 4. Remove exactly 1 from the inventory stack
            inventory.RemoveItem(item, 1);

            Refresh();
        }
    }
}

