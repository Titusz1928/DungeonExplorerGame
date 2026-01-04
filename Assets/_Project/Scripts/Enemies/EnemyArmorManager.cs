using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public class EnemyArmorManager : MonoBehaviour
{
    private EnemyController controller;

    // Organizes items by Slot and Layer, identical to the Player's system
    public Dictionary<ArmorSlot, Dictionary<ArmorLayer, ItemInstance>> equippedArmor = new();
    public List<ItemInstance> rawInventory = new();

    public void Initialize(EnemyController owner)
    {
        controller = owner;

        // 1. Initialize the dictionary
        foreach (ArmorSlot slot in System.Enum.GetValues(typeof(ArmorSlot)))
        {
            if (slot == ArmorSlot.None) continue;
            equippedArmor[slot] = new Dictionary<ArmorLayer, ItemInstance>();
        }

        // 2. Generate Loot
        if (controller.data.corpseContainer is GroupedContainerSO groupedSO)
        {
            rawInventory = groupedSO.GenerateLoot();

            // 3. Auto-Equip anything that is armor
            foreach (var item in rawInventory)
            {
                if (item.itemSO is ArmorItemSO armor)
                {
                    EquipArmor(item, armor);
                }
            }
        }
    }

    private void EquipArmor(ItemInstance instance, ArmorItemSO data)
    {
        // For enemies, we just put it in the highest layer slot. 
        // If an enemy has two "Torso" plate armors, the last one generated wins.
        foreach (ArmorSlot slot in GetSlotsFromFlags(data.slotsCovered))
        {
            equippedArmor[slot][data.layer] = instance;
        }
    }

    public double GetProtection(string partName, DamageType type)
    {
        double totalProtection = 0;
        var part = controller.data.anatomy.Find(p => p.partName == partName);
        if (part == null) return 0;

        totalProtection += GetValueFromDefense(part.naturalDefense, type);

        if (equippedArmor.TryGetValue(part.associatedSlot, out var layers))
        {
            foreach (var layerPair in layers)
            {
                ItemInstance item = layerPair.Value;
                ArmorItemSO armorSO = (ArmorItemSO)item.itemSO;

                var slotDef = armorSO.defenses.Find(d => d.slot == part.associatedSlot);
                if (slotDef != null)
                {
                    float baseVal = GetValueFromDefense(slotDef.defense, type);
                    // Protection scales based on durability percentage (0.0 to 1.0)
                    double durabilityPercent = (double)item.currentDurability / armorSO.maxDurability;
                    totalProtection += baseVal * durabilityPercent;
                }
            }
        }
        return totalProtection;
    }

    private float GetValueFromDefense(ArmorDefense def, DamageType type)
    {
        return type switch
        {
            DamageType.Blunt => def.blunt,
            DamageType.Pierce => def.pierce,
            DamageType.Slash => def.slash,
            _ => 0
        };
    }

    private IEnumerable<ArmorSlot> GetSlotsFromFlags(ArmorSlot flags)
    {
        foreach (ArmorSlot slot in System.Enum.GetValues(typeof(ArmorSlot)))
        {
            if (slot != ArmorSlot.None && flags.HasFlag(slot))
                yield return slot;
        }
    }
}