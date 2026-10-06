using TitusGames.Framework;
using UnityEngine;

public static class ConsumableUseHandler
{
    public static void UseItem(ItemInstance item)
    {
        if (item.itemSO is not ConsumableItemSO consumable) return;

        ServiceLocator.Current.TryGet<IPlayerStateService>(out var playerState);
        ServiceLocator.Current.TryGet<IMessageService>(out var messageService);

        if (playerState == null)
        {
            Debug.LogError("[ConsumableUseHandler] IPlayerStateService is not registered!");
            return;
        }

        // Access attached components cleanly via the interface's GameObject property
        Inventory inventory = playerState.GameObject.GetComponent<Inventory>();
        if (inventory == null)
        {
            Debug.LogError("[ConsumableUseHandler] Inventory component missing on player object!");
            return;
        }

        bool itemUsed = false;

        if (consumable.consumableType == ConsumableType.Food || consumable.consumableType == ConsumableType.Potion)
        {
            playerState.AddHealth(consumable.healthAmount);
            playerState.AddStamina(consumable.staminaAmount);

            messageService?.ShowMessageDirectly( $"+{consumable.healthAmount} HP  +{consumable.staminaAmount} STAMINA", "UI/Icons/heal");

            itemUsed = true;
        }
        else if (consumable.consumableType == ConsumableType.FirstAid)
        {
            InjuryManager injuryManager = playerState.GameObject.GetComponent<InjuryManager>();
            Injury target = injuryManager?.GetFirstTreatableInjury();

            if (target != null)
            {
                injuryManager.ApplyBandage(target);
                messageService?.ShowMessageDirectly($"Bandaged {target.bodyPart}", "UI/Icons/heal");
                itemUsed = true;
            }
            else
            {
                messageService?.ShowMessageDirectly("No injuries require bandaging.", null);
            }
        }

        if (itemUsed)
        {
            inventory.RemoveItem(item, 1);
            HandlePostUseRefreshes();

            // Battle turn economy
            ServiceLocator.Current.TryGet<IUIManagerService>(out var uimanagerService);
            if (uimanagerService.IsInBattle)
            {
                if (ServiceLocator.Current.TryGet<IBattleService>(out var battleService))
                {
                    battleService.SetPendingAction(PlayerActionType.Treatment);
                    battleService.EndTurn();
                }
            }
        }
    }

    private static void HandlePostUseRefreshes()
    {
        // 1. Refresh Injury Dropdowns (These stay specific as they use Injury data)
        foreach (var ui in Object.FindObjectsByType<InjuryDropdownUI>(FindObjectsSortMode.None))
        {
            if (ui.gameObject.activeInHierarchy) ui.RefreshInjuries();
        }

        // 2. Refresh ALL Category Dropdowns (Consumables, Weapons, Shields, etc.)
        foreach (var ui in Object.FindObjectsByType<CategoryDropdownUI>(FindObjectsSortMode.None))
        {
            if (ui.gameObject.activeInHierarchy) ui.Refresh();
        }

        // 3. Refresh main Inventory window if open
        var invWindow = Object.FindFirstObjectByType<InventoryWindow>();
        if (invWindow != null) invWindow.Refresh();
    }
}