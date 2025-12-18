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
            save.inventory.items.Add(new ItemSaveEntry
            {
                itemID = item.itemSO.ID,
                quantity = item.quantity,
                durability = item.currentDurability
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
}
