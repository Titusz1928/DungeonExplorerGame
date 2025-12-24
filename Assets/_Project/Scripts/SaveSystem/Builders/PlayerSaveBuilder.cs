using UnityEngine;

public static class PlayerSaveBuilder
{
    public static PlayerSave Build()
    {
        PlayerSave save = new PlayerSave();

        Transform player = PlayerReference.PlayerTransform;

        // --------------------
        // POSITION
        // --------------------
        save.position = player.position;

        // --------------------
        // PLAYER STATE
        // --------------------
        var state = PlayerStateManager.Instance;
        save.health = state.health;
        save.stamina = state.stamina;

        // --------------------
        // INVENTORY
        // --------------------
        // Make sure inventory is not null
        if (save.inventory == null)
            save.inventory = new InventorySaveData();

        Inventory inv = player.GetComponent<Inventory>();
        foreach (var item in inv.items)
        {
            save.inventory.entries.Add(new ItemSaveEntry
            {
                itemID = item.itemSO.ID,
                quantity = item.quantity,
                durability = item.currentDurability,
                isEquipped = item.isEquipped,
            });
        }

        // --------------------
        // EQUIPMENT
        // --------------------
        save.equipment = EquipmentSaveBuilder.Build();

        // --------------------
        // SKILLS
        // -------------------- 
        save.skills = SkillSaveBuilder.Build();

        return save;
    }

    public static void Apply(GameObject playerObj, PlayerSave save)
    {
        if (playerObj == null || save == null) return;

        // 1. POSITION
        playerObj.transform.position = save.position;

        // 2. PLAYER STATE (Health & Stamina)
        var state = playerObj.GetComponent<PlayerStateManager>();
        if (state != null)
        {
            state.health = save.health;
            state.stamina = save.stamina;
            // Trigger a UI update if your state manager has an event for this
        }

        // 3. INVENTORY
        Inventory inv = playerObj.GetComponent<Inventory>();
        if (inv != null && save.inventory != null)
        {
            inv.items.Clear();
            foreach (var entry in save.inventory.entries)
            {
                ItemSO itemSO = ItemDatabase.instance.GetByID(entry.itemID);
                if (itemSO != null)
                {
                    inv.items.Add(new ItemInstance(itemSO, entry.quantity)
                    {
                        currentDurability = entry.durability
                    });
                }
            }
        }

        // 4. EQUIPMENT
        var equipmentManager = playerObj.GetComponent<EquipmentManager>();

        if (equipmentManager != null && inv != null)
        {
            // Pass 'inv' as a new argument
            EquipmentSaveBuilder.Apply(equipmentManager, inv, save.equipment);
        }

        // 5. SKILLS
        SkillSaveBuilder.Apply(save.skills);
    }

}
