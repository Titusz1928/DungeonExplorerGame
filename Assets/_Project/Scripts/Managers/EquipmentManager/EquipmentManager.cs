using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance;

    // Armor is stored by Slot → Layer → Item
    private Dictionary<ArmorSlot, Dictionary<ArmorLayer, ItemInstance>> equippedArmor;

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
        Debug.Log($"Equipped {armor.itemName}");
    }

    // -----------------------
    // UNEQUIPPING ARMOR
    // -----------------------
    public void UnequipArmor(ItemInstance item)
    {
        ArmorItemSO armor = item.itemSO as ArmorItemSO;
        if (armor == null) return;

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

        foreach (ArmorLayer layer in System.Enum.GetValues(typeof(ArmorLayer)))
        {
            if (layer == ArmorLayer.Under || layer == ArmorLayer.Chainmail || layer == ArmorLayer.Plate || layer == ArmorLayer.Over)
            {
                if (equippedArmor[slot].TryGetValue(layer, out ItemInstance armorItem))
                {
                    if (armorItem.itemSO is ArmorItemSO armorSO)
                    {
                        var slotDefense = armorSO.defenses.Find(d => d.slot == slot);
                        if (slotDefense.slot != ArmorSlot.None)
                        {
                            total.blunt += slotDefense.defense.blunt;
                            total.pierce += slotDefense.defense.pierce;
                            total.slash += slotDefense.defense.slash;
                        }
                    }
                }
            }
        }

        return total;
    }

    public float GetTotalSneak()
    {
        float totalSneak = 0f;

        foreach (var slotDict in equippedArmor.Values)
        {
            foreach (var item in slotDict.Values)
            {
                if (item.itemSO is ArmorItemSO armor)
                {
                    totalSneak += armor.noise; // assuming noise reduces sneak
                }
            }
        }

        return totalSneak;
    }

    public float GetTotalConspicuousness()
    {
        float totalConspicuous = 0f;

        foreach (var slotDict in equippedArmor.Values)
        {
            foreach (var item in slotDict.Values)
            {
                if (item.itemSO is ArmorItemSO armor)
                {
                    totalConspicuous += armor.conspicuousness;
                }
            }
        }

        return totalConspicuous;
    }

}

