using System.Collections.Generic;
using TitusGames.Framework;
using UnityEngine;

public class InventoryWindow : MonoBehaviour
{
    [Header("Container")]
    [SerializeField] private Transform rowContainer;

    [Header("Inventory Items Tab")]
    [SerializeField] private GameObject itemsRowPrefab;
    [SerializeField] private GameObject itemsTitleRowPrefab;
    [SerializeField] private GameObject itemsStatsRowPrefab;
    [SerializeField] private GameObject moveWindow;
    [SerializeField] private GameObject itemInfoWindow;
    [SerializeField] private GameObject dropWindow;
    [SerializeField] private GameObject ripClothingWindowPrefab;
    [SerializeField] private GameObject documentWindowPrefab;

    [Header("Inventory Equipment Tab")]
    [SerializeField] private GameObject equipmentTitleRowPrefab;
    [SerializeField] private GameObject equipmentRowPrefab;
    [SerializeField] private GameObject equipmentStatRowPrefab;

    [Header("Inventory Health & Skills Tab")]
    [SerializeField] private GameObject injurySectionPrefab;
    [SerializeField] private GameObject skillsRowPrefab;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Inventory inventory;

    private short tabSelected;

    public void Initialize(Inventory inv, Transform playerTransform)
    {
        inventory = inv;
        player = playerTransform;
        tabSelected = 0;
        Refresh();
    }

    public void Refresh()
    {
        if (inventory == null)
        {
            Debug.LogError("[InventoryWindow] Inventory is not assigned!");
            return;
        }

        switch (tabSelected)
        {
            case 0:
                BuildItemSection();
                break;
            case 1:
                BuildEquipmentSection();
                break;
            case 2:
                BuildHealthNSkillsSection();
                break;
            default:
                Debug.LogWarning($"[InventoryWindow] Invalid tabSelected index: {tabSelected}");
                break;
        }
    }

    // -----------------------
    // TAB SELECTION (UI Callbacks)
    // -----------------------
    public void ItemsTabSelected()
    {
        Debug.Log("[InventoryWindow] ItemsTabSelected() called from UI Button click.");
        tabSelected = 0;
        Refresh();
    }

    public void EquipmentTabSelected()
    {
        Debug.Log("[InventoryWindow] EquipmentTabSelected() called from UI Button click.");
        tabSelected = 1;
        Refresh();
    }

    public void StatsNSkillsTabSelected()
    {
        Debug.Log("[InventoryWindow] StatsNSkillsTabSelected() called from UI Button click.");
        tabSelected = 2;
        Refresh();
    }

    // -----------------------
    // SECTION BUILDERS
    // -----------------------
    public void BuildItemSection()
    {
        Debug.Log("[InventoryWindow] Building Items Section...");
        if (!ValidateContainerAndPrefab(itemsRowPrefab)) return;

        ClearRowContainer();

        if (itemsStatsRowPrefab != null)
        {
            GameObject rowObj = Instantiate(itemsStatsRowPrefab, rowContainer);
            if (rowObj.TryGetComponent<ItemStatRow>(out var row))
            {
                row.SetData(inventory, this);
            }
        }
        else
        {
            Debug.LogWarning("[InventoryWindow] itemsStatsRowPrefab not assigned!");
        }

        foreach (ItemInstance instance in inventory.items)
        {
            GameObject rowObj = Instantiate(itemsRowPrefab, rowContainer);
            if (rowObj.TryGetComponent<InventoryRow>(out var row))
            {
                row.SetData(instance, this);
            }
        }
    }

    public void BuildEquipmentSection()
    {
        if (rowContainer == null) return;

        ClearRowContainer();

        if (equipmentTitleRowPrefab != null)
        {
            Instantiate(equipmentTitleRowPrefab, rowContainer);
        }

        if (!ServiceLocator.Current.TryGet<IEquipmentService>(out var equipmentService))
        {
            Debug.LogError("[InventoryWindow] IEquipmentService not registered in ServiceLocator!");
            return;
        }

        // Iterate through all body part slots
        foreach (ArmorSlot slot in System.Enum.GetValues(typeof(ArmorSlot)))
        {
            if (slot == ArmorSlot.None) continue;

            ArmorDefense totalDefense = equipmentService.GetTotalDefenseForSlot(slot);

            if (equipmentRowPrefab != null)
            {
                GameObject rowObj = Instantiate(equipmentRowPrefab, rowContainer);
                if (rowObj.TryGetComponent<EquipmentInventoryRow>(out var row))
                {
                    row.SetData(
                        slot.ToString(),
                        totalDefense.blunt,
                        totalDefense.pierce,
                        totalDefense.slash
                    );
                }
            }
        }

        // Stats rows (Sneak & Conspicuousness)
        if (equipmentStatRowPrefab != null)
        {
            GameObject sneakRow = Instantiate(equipmentStatRowPrefab, rowContainer);
            if (sneakRow.TryGetComponent<EquipmentStatRow>(out var sneakStat))
            {
                sneakStat.SetData("Sneak", equipmentService.GetTotalSneak());
            }

            GameObject consRow = Instantiate(equipmentStatRowPrefab, rowContainer);
            if (consRow.TryGetComponent<EquipmentStatRow>(out var consStat))
            {
                consStat.SetData("Conspicuousness", equipmentService.GetTotalConspicuousness());
            }
        }
    }

    public void BuildHealthNSkillsSection()
    {
        if (rowContainer == null) return;

        ClearRowContainer();

        if (ServiceLocator.Current.TryGet<IPlayerStateService>(out var playerState))
        {
            GameObject playerObj = playerState.GameObject;
            if (playerObj != null && playerObj.TryGetComponent<InjuryManager>(out var injuryManager))
            {
                if (injurySectionPrefab != null && injuryManager.activeInjuries.Count > 0)
                {
                    GameObject healthObj = Instantiate(injurySectionPrefab, rowContainer);
                    if (healthObj.TryGetComponent<InjuryDropdownUI>(out var dropdown))
                    {
                        dropdown.Initialize(playerObj);
                    }
                }
            }
        }

        foreach (PlayerSkill skill in System.Enum.GetValues(typeof(PlayerSkill)))
        {
            if (skillsRowPrefab == null) continue;

            GameObject rowObj = Instantiate(skillsRowPrefab, rowContainer);
            if (rowObj.TryGetComponent<InventorySkillRow>(out var row))
            {
                row.SetData(skill);
            }
        }
    }

    // -----------------------
    // ITEM ACTIONS
    // -----------------------
    public void OnInfoButtonPressed(ItemInstance item)
    {
        GameObject windowGO = OpenWindow(itemInfoWindow);
        if (windowGO != null && windowGO.TryGetComponent<ItemInfoWindow>(out var infoWindow))
        {
            infoWindow.Show(item);
        }
    }


    public void OnUseButtonPressed(ItemInstance item)
    {
        if (item == null) return;

        // Resolve services at the start to avoid redundant lookup calls
        ServiceLocator.Current.TryGet<IMessageService>(out var messageService);
        ServiceLocator.Current.TryGet<IEquipmentService>(out var equipmentService);

        ItemSO so = item.itemSO;

        // --- UNEQUIP CHECK ---
        if (item.isEquipped)
        {
            if (equipmentService != null)
            {
                equipmentService.Unequip(item);
                messageService?.ShowMessageDirectly($"Unequipped {so.itemName}");
                Refresh();
            }
            return;
        }

        // --- EQUIP / CONSUME CHECK ---
        switch (so)
        {
            case ConsumableItemSO consumable:
                HandleConsumableUse(item, consumable, messageService);
                break;

            case DocumentSO:
                OnDocumentUsed(item);
                break;

            case WeaponItemSO weapon:
                if (item.currentDurability <= 0)
                {
                    messageService?.ShowMessageDirectly($"{weapon.itemName} is broken and cannot be equipped!");
                    return;
                }
                if (equipmentService != null)
                {
                    equipmentService.EquipMainHand(item);
                    messageService?.ShowMessageDirectly($"Equipped {weapon.itemName}");
                    Refresh();
                }
                break;

            case ShieldItemSO shield:
                if (item.currentDurability <= 0)
                {
                    messageService?.ShowMessageDirectly($"{shield.itemName} is destroyed!");
                    return;
                }
                if (equipmentService != null)
                {
                    equipmentService.EquipShield(item);
                    messageService?.ShowMessageDirectly($"Equipped {shield.itemName}");
                    Refresh();
                }
                break;

            case ArmorItemSO armor:
                if (item.currentDurability <= 0)
                {
                    messageService?.ShowMessageDirectly($"{armor.itemName} is too damaged to wear!");
                    return;
                }
                if (equipmentService != null)
                {
                    equipmentService.EquipArmor(item);
                    messageService?.ShowMessageDirectly($"Equipped {armor.itemName}");
                    Refresh();
                }
                break;

            case ItemSO genericItem when genericItem.ID == 20: // Scissors
                if (item.currentDurability <= 0)
                {
                    messageService?.ShowMessageDirectly("These scissors are broken and cannot cut anything.");
                    return;
                }
                OnRipButtonPressed(item);
                break;

            default:
                messageService?.ShowMessageDirectly($"{so.itemName} cannot be used");
                break;
        }
    }

    private void HandleConsumableUse(ItemInstance item, ConsumableItemSO consumable, IMessageService messageService = null)
    {
        // Fallback service resolution if called independently without passing the service
        if (messageService == null)
        {
            ServiceLocator.Current.TryGet<IMessageService>(out messageService);
        }

        if (!ServiceLocator.Current.TryGet<IPlayerStateService>(out var playerState))
        {
            Debug.LogError("[InventoryWindow] IPlayerStateService is missing!");
            return;
        }

        if (consumable.consumableType == ConsumableType.Food || consumable.consumableType == ConsumableType.Potion)
        {
            playerState.AddHealth(consumable.healthAmount);
            playerState.AddStamina(consumable.staminaAmount);

            if (ServiceLocator.Current.TryGet<IGameAudioService>(out var audioService))
            {
                audioService.PlaySFX(consumable.useSound);
            }

            inventory.RemoveItem(item);

            if (consumable.returnItem != null)
            {
                bool added = inventory.AddItem(consumable.returnItem, 1);
                if (!added)
                {
                    messageService?.ShowMessageDirectly("Inventory full! Discarded empty container.");
                    Vector3 spawnPos = PlayerReference.PlayerTransform.position;
                    ItemInstance singleDrop = new ItemInstance(consumable.returnItem, 1)
                    {
                        currentDurability = item.currentDurability
                    };
                    ItemSpawner.Instance.SpawnWorldItem(singleDrop, spawnPos);
                }
            }

            Sprite infoIcon = Resources.Load<Sprite>("UI/Icons/heal");
            messageService?.ShowMessageWithSprite(
                $"+{consumable.healthAmount} HP   +{consumable.staminaAmount} STAMINA",
                infoIcon,
                useLocalization: false
            );

            Refresh();
        }
        else if (consumable.consumableType == ConsumableType.FirstAid)
        {
            InjuryManager injuryManager = playerState.GameObject.GetComponent<InjuryManager>();
            Injury target = injuryManager?.GetFirstTreatableInjury();

            if (target != null)
            {
                injuryManager.ApplyBandage(target);
                inventory.RemoveItem(item);

                Sprite infoIcon = Resources.Load<Sprite>("UI/Icons/heal");
                messageService?.ShowMessageWithSprite(
                    $"Bandaged {target.bodyPart} ({target.type})",
                    infoIcon,
                    useLocalization: false
                );

                Refresh();
            }
            else
            {
                messageService?.ShowMessageDirectly("No injuries require bandaging.");
            }
        }
    }

    public void OnDocumentUsed(ItemInstance item)
    {
        GameObject windowGO = OpenWindow(documentWindowPrefab);
        if (windowGO != null && windowGO.TryGetComponent<DocumentWindow>(out var docWindow))
        {
            docWindow.OpenDocument(item);
        }
    }

    public void OnRipButtonPressed(ItemInstance item)
    {
        GameObject windowGO = OpenWindow(ripClothingWindowPrefab);
        if (windowGO != null && windowGO.TryGetComponent<RipClothingWindow>(out var ripWindow))
        {
            ripWindow.OpenRipWindow(item, inventory);
        }
    }

    public void OnMoveButtonPressed(ItemInstance item)
    {
        GameObject windowGO = OpenWindow(moveWindow);
        if (windowGO != null && windowGO.TryGetComponent<MoveWindow>(out var moveWindowInstance))
        {
            moveWindowInstance.OpenMoveWindow(item, inventory, player);
        }
    }

    public void OnDropButtonPressed(ItemInstance item, bool showWindow)
    {
        if (item == null || inventory == null) return;

        if (showWindow && item.quantity > 1)
        {
            GameObject windowGO = OpenWindow(dropWindow);
            if (windowGO != null && windowGO.TryGetComponent<DropWindow>(out var dropWindowInstance))
            {
                dropWindowInstance.OpenDropWindow(item, inventory, player);
            }
        }
        else
        {
            if (item.isEquipped && ServiceLocator.Current.TryGet<IEquipmentService>(out var equipmentService))
            {
                equipmentService.Unequip(item);
            }

            Vector3 spawnPos = PlayerReference.PlayerTransform.position;
            ItemInstance singleDrop = new ItemInstance(item.itemSO, 1)
            {
                currentDurability = item.currentDurability
            };

            ItemSpawner.Instance.SpawnWorldItem(singleDrop, spawnPos);
            inventory.RemoveItem(item, 1);

            Refresh();
        }
    }

    // -----------------------
    // HELPERS
    // -----------------------
    private void ClearRowContainer()
    {
        if (rowContainer == null) return;

        foreach (Transform child in rowContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private GameObject OpenWindow(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("[InventoryWindow] Attempted to open null window prefab!");
            return null;
        }

        if (ServiceLocator.Current.TryGet<IWindowService>(out var windowService))
        {
            return windowService.OpenWindow(prefab);
        }

        Debug.LogError("[InventoryWindow] IWindowService is not registered in ServiceLocator!");
        return null;
    }

    private bool ValidateContainerAndPrefab(GameObject prefab)
    {
        if (rowContainer == null || prefab == null)
        {
            Debug.LogError("[InventoryWindow] Row container or required prefab is missing!");
            return false;
        }
        return true;
    }
}