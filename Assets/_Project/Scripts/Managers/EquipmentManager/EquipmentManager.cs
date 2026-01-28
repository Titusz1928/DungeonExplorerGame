using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance;

    public PlayerPaperDoll playerVisuals;

    // Armor is stored by Slot → Layer → Item
    public Dictionary<ArmorSlot, Dictionary<ArmorLayer, ItemInstance>> equippedArmor;

    // Weapons
    public ItemInstance mainHandWeapon;
    public ItemInstance offHandShield;

    private void Awake()
    {
        Instance = this;

        // Initialize armor dictionary
        equippedArmor = new Dictionary<ArmorSlot, Dictionary<ArmorLayer, ItemInstance>>();

        foreach (ArmorSlot slot in System.Enum.GetValues(typeof(ArmorSlot)))
        {
            if (slot == ArmorSlot.None) continue;
            equippedArmor[slot] = new Dictionary<ArmorLayer, ItemInstance>();
        }
    }

    // -----------------------
    // EQUIPPING ARMOR
    // -----------------------
    public void EquipArmor(ItemInstance item)
    {
        ArmorItemSO armor = item.itemSO as ArmorItemSO;
        if (armor == null)
        {
            Debug.LogWarning("Tried to equip non-armor as armor.");
            return;
        }

        AudioManager.Instance.PlaySFX(armor.useSound);

        foreach (ArmorSlot slot in GetSlotsFromFlags(armor.slotsCovered))
        {
            // Each slot can have 1 item per layer
            var layers = equippedArmor[slot];

            // If something is already equipped in this layer, unequip it
            if (layers.ContainsKey(armor.layer))
            {
                UnequipArmorSlotLayer(slot, armor.layer);
            }

            layers[armor.layer] = item;
        }

        item.isEquipped = true;
        Debug.Log($"[EQUIPMENT MANAGER]: Equipped {armor.itemName}");

        if (playerVisuals != null)
        {
            Debug.Log($"[EQUIP] Calling PaperDoll: Slot={armor.primaryVisualSlot}, Layer={armor.layer}, Frames={armor.animationFrames?.Length}");
            playerVisuals.SetSlotVisual(armor.primaryVisualSlot, armor.layer, armor.animationFrames);
        }
        else
        {
            Debug.LogError("[EQUIP] playerVisuals is NULL in EquipmentManager!");
        }
    }

    public void Equip(ItemInstance item)
    {
        if (item == null) return;

        // Route based on the ScriptableObject type
        if (item.itemSO is WeaponItemSO)
        {
            EquipMainHand(item);
        }
        else if (item.itemSO is ShieldItemSO)
        {
            EquipShield(item);
        }
        else if (item.itemSO is ArmorItemSO)
        {
            EquipArmor(item);
        }
    }


    public void Unequip(ItemInstance item)
    {
        if (item == null) return;

        // Route based on the ScriptableObject type
        if (item.itemSO is WeaponItemSO)
        {
            UnequipMainHand(item);
        }
        else if (item.itemSO is ShieldItemSO)
        {
            UnequipShield(item);
        }
        else if (item.itemSO is ArmorItemSO)
        {
            UnequipArmor(item);
        }
    }


    // -----------------------
    // UNEQUIPPING ARMOR
    // -----------------------
    public void UnequipArmor(ItemInstance item)
    {
        ArmorItemSO armor = item.itemSO as ArmorItemSO;
        if (armor == null) return;

        AudioManager.Instance.PlaySFX(armor.useSound);

        foreach (ArmorSlot slot in GetSlotsFromFlags(armor.slotsCovered))
        {
            UnequipArmorSlotLayer(slot, armor.layer);
        }

        item.isEquipped = false;
        Debug.Log($"Unequipped {armor.itemName}");
    }

    private void UnequipArmorSlotLayer(ArmorSlot slot, ArmorLayer layer)
    {
        if (equippedArmor[slot].ContainsKey(layer))
        {
            equippedArmor[slot][layer].isEquipped = false;
            equippedArmor[slot].Remove(layer);
        }

        if (playerVisuals != null)
        {
            // Now we only clear the specific slot!
            playerVisuals.ClearSlotVisual(slot, layer);
        }
    }

    public void UnequipMainHand(ItemInstance weapon)
    {
        if (mainHandWeapon == weapon)
        {
            weapon.isEquipped = false;
            mainHandWeapon = null;
            Debug.Log($"Unequipped weapon: {weapon.itemSO.itemName}");
        }
    }

    public void UnequipShield(ItemInstance shield)
    {
        if (offHandShield == shield)
        {
            shield.isEquipped = false;
            offHandShield = null;
            Debug.Log($"Unequipped shield: {shield.itemSO.itemName}");
        }
    }

    // -----------------------
    // WEAPONS
    // -----------------------
    public void EquipMainHand(ItemInstance weapon)
    {
        if (mainHandWeapon != null)
            mainHandWeapon.isEquipped = false;

        mainHandWeapon = weapon;
        weapon.isEquipped = true;

        Debug.Log($"Equipped weapon: {weapon.itemSO.itemName}");
    }

    public void EquipShield(ItemInstance shield)
    {
        if (offHandShield != null)
            offHandShield.isEquipped = false;

        offHandShield = shield;
        shield.isEquipped = true;

        Debug.Log($"Equipped shield: {shield.itemSO.itemName}");
    }

    // -----------------------
    // HELPERS
    // -----------------------
    private IEnumerable<ArmorSlot> GetSlotsFromFlags(ArmorSlot flags)
    {
        foreach (ArmorSlot slot in System.Enum.GetValues(typeof(ArmorSlot)))
        {
            if (slot != ArmorSlot.None && flags.HasFlag(slot))
                yield return slot;
        }
    }

    public bool IsArmorEquipped(ArmorSlot slot, ArmorLayer layer)
    {
        return equippedArmor[slot].ContainsKey(layer);
    }

    public ArmorDefense GetTotalDefenseForSlot(ArmorSlot slot)
    {
        ArmorDefense total = new ArmorDefense();

        foreach (var layerPair in equippedArmor[slot])
        {
            ItemInstance armorItem = layerPair.Value;
            if (armorItem.itemSO is ArmorItemSO armorSO)
            {
                var slotDefense = armorSO.defenses.Find(d => d.slot == slot);
                if (slotDefense.slot != ArmorSlot.None)
                {
                    // 1. Calculate durability percentage (0.0 to 1.0)
                    double durabilityPercent = armorItem.currentDurability / armorSO.maxDurability;

                    // 2. Scale protection (e.g., 50% durability = 50% protection)
                    total.blunt += (float)(slotDefense.defense.blunt * durabilityPercent);
                    total.pierce += (float)(slotDefense.defense.pierce * durabilityPercent);
                    total.slash += (float)(slotDefense.defense.slash * durabilityPercent);
                }
            }
        }
        return total;
    }

    public float GetTotalSneak()
    {
        float totalNoise = 0f;

        foreach (var slotPair in equippedArmor)
        {
            float maxNoiseForSlot = 0f;

            foreach (var item in slotPair.Value.Values)
            {
                if (item.itemSO is ArmorItemSO armor)
                {
                    if (armor.noise > maxNoiseForSlot)
                        maxNoiseForSlot = armor.noise;
                }
            }

            totalNoise += maxNoiseForSlot;
        }

        return totalNoise;
    }

    private static readonly ArmorLayer[] LayerPriority =
    {
        ArmorLayer.Under,
        ArmorLayer.Chainmail,
        ArmorLayer.Plate,
        ArmorLayer.Over
    };

    public float GetTotalConspicuousness()
    {
        float total = 0f;

        foreach (var slotPair in equippedArmor)
        {
            var layers = slotPair.Value;

            // Find highest equipped layer for this slot
            for (int i = LayerPriority.Length - 1; i >= 0; i--)
            {
                ArmorLayer layer = LayerPriority[i];

                if (layers.TryGetValue(layer, out ItemInstance item) &&
                    item.itemSO is ArmorItemSO armor)
                {
                    total += armor.conspicuousness;
                    break; // 🔑 only top layer counts
                }
            }
        }

        return total;
    }

}

