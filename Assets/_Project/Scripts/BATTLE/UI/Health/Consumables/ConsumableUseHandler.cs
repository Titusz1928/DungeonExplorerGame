using UnityEngine;

public static class ConsumableUseHandler
{
    public static void UseItem(ItemInstance item)
    {
        if (item.itemSO is not ConsumableItemSO consumable) return;

        Inventory inventory = PlayerStateManager.Instance.GetComponent<Inventory>();
        bool itemUsed = false;

        if (consumable.consumableType == ConsumableType.Food || consumable.consumableType == ConsumableType.Potion)
        {
            PlayerStateManager.Instance.addHealth(consumable.healthAmount);
            PlayerStateManager.Instance.addStamina(consumable.staminaAmount);

            MessageManager.Instance.ShowMessageDirectly(
                $"+{consumable.healthAmount} HP  +{consumable.staminaAmount} STAMINA",
                Resources.Load<Sprite>("UI/Icons/heal"));

            itemUsed = true;
        }
        else if (consumable.consumableType == ConsumableType.FirstAid)
        {
            InjuryManager injuryManager = PlayerStateManager.Instance.GetComponent<InjuryManager>();
            Injury target = injuryManager.GetFirstTreatableInjury();

            if (target != null)
            {
                injuryManager.ApplyBandage(target);
                MessageManager.Instance.ShowMessageDirectly($"Bandaged {target.bodyPart}", Resources.Load<Sprite>("UI/Icons/heal"));
                itemUsed = true;
            }
            else
            {
                MessageManager.Instance.ShowMessageDirectly("No injuries require bandaging.", null);
            }
        }

        if (itemUsed)
        {
            inventory.RemoveItem(item, 1);
            HandlePostUseRefreshes();

            // Battle turn economy
            if (UIManager.Instance.IsInBattle)
            {
                BattleManager.Instance.SetPendingAction(PlayerActionType.Treatment);
                BattleManager.Instance.OnEndTurnPressed();
            }
        }
    }

    private static void HandlePostUseRefreshes()
    {
        // Refresh all Dropdowns (Injuries and Consumables)
        foreach (var ui in Object.FindObjectsByType<InjuryDropdownUI>(FindObjectsSortMode.None))
            if (ui.gameObject.activeInHierarchy) ui.RefreshInjuries();

        foreach (var ui in Object.FindObjectsByType<ConsumableDropdownUI>(FindObjectsSortMode.None))
            if (ui.gameObject.activeInHierarchy) ui.RefreshConsumables();

        // Refresh main Inventory window if open
        var invWindow = Object.FindFirstObjectByType<InventoryWindow>();
        if (invWindow != null) invWindow.Refresh();
    }
}