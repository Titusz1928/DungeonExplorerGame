using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemInfoWindow : MonoBehaviour
{
    public Transform content;
    public GameObject rowPrefab;

    private ItemInstance currentItemInstance;

    public void Show(ItemInstance instance)
    {
        currentItemInstance = instance;
        ClearRows();

        AddBasicInfo(instance);

        if (instance.itemSO is ArmorItemSO armor)
            AddArmorInfo(armor, instance);

        if (instance.itemSO is ConsumableItemSO consumable)
            AddConsumableInfo(consumable);

        if (instance.itemSO is WeaponItemSO weapon)
            AddWeaponInfo(weapon);

        if (instance.itemSO is DocumentSO document)
            AddDocumentInfo(document);

        //if (instance.itemSO is ConsumableItemSO consumable)
        //    AddConsumableInfo(consumable);

        // Add more types as needed
    }

    private void ClearRows()
    {
        foreach (Transform child in content)
            Destroy(child.gameObject);
    }

    // ----------------------------
    // CREATE ROW HELPERS
    // ----------------------------

    private void AddRow(string leftText, string rightText = "")
    {
        GameObject row = Instantiate(rowPrefab, content);

        // Find the two RowCells
        Transform leftCell = row.transform.GetChild(0); // first RowCell
        Transform rightCell = row.transform.GetChild(1); // second RowCell

        // Set the text for the left cell
        TextMeshProUGUI leftTMP = leftCell.GetComponentInChildren<TextMeshProUGUI>();
        if (leftTMP != null)
            leftTMP.text = leftText;

        // Set the text for the right cell
        TextMeshProUGUI rightTMP = rightCell.GetComponentInChildren<TextMeshProUGUI>();
        if (!string.IsNullOrEmpty(rightText))
        {
            if (rightTMP != null)
                rightTMP.text = rightText;
            rightCell.gameObject.SetActive(true);
        }
        else
        {
            rightCell.gameObject.SetActive(false);
        }
    }

    // ----------------------------
    // BASIC ITEM INFO
    // ----------------------------
    private void AddBasicInfo(ItemInstance instance)
    {
        ItemSO item = instance.itemSO;

        AddRow("Name", item.itemName);
        AddRow("Category", item.category.ToString());

        if (item.isBreakable)
        {
            AddRow(
                "Durability",
                $"{instance.currentDurability} / {item.maxDurability}"
            );
        }

        if (item.isStackable)
            AddRow("Stack Size", item.maxStackSize.ToString());
    }

    // ----------------------------
    // ARMOR SPECIFIC INFO
    // ----------------------------
    private void AddArmorInfo(ArmorItemSO armor, ItemInstance instance)
    {
        AddRow("Armor Type", armor.layer.ToString());

        // ----------------------------
        // Slots Covered
        // ----------------------------
        // Convert the Flags enum into readable names
        List<string> coveredSlots = new List<string>();

        foreach (ArmorSlot slot in System.Enum.GetValues(typeof(ArmorSlot)))
        {
            if (slot != ArmorSlot.None && armor.slotsCovered.HasFlag(slot))
                coveredSlots.Add(slot.ToString());
        }

        AddRow("Covers", string.Join(", ", coveredSlots));

        // ----------------------------
        // Defense Per Slot
        // ----------------------------
        foreach (var def in armor.defenses)
        {
            string slotName = def.slot.ToString();

            AddRow($"{slotName} Blunt", def.defense.blunt.ToString());
            AddRow($"{slotName} Pierce", def.defense.pierce.ToString());
            AddRow($"{slotName} Slash", def.defense.slash.ToString());
        }

        // ----------------------------
        // Stealth
        // ----------------------------
        AddRow("Noise", armor.noise.ToString());
        AddRow("Conspicuous", armor.conspicuousness.ToString());

        // ----------------------------
        // Wear & Tear (from instance)
        // ----------------------------
        if (instance != null)
            AddRow("Holes", instance.holes.ToString());
    }

    // -------------------------------------
    //  WEAPON INFO
    // -------------------------------------
    private void AddWeaponInfo(WeaponItemSO weapon)
    {
        AddRow("Weapon Type", weapon.weaponType.ToString());
        AddRow("Damage Type", weapon.damageType.ToString());
        AddRow("Damage", weapon.damageAmount.ToString("F2"));
    }

    // -------------------------------------
    //  CONSUMABLE INFO
    // -------------------------------------
    private void AddConsumableInfo(ConsumableItemSO c)
    {
        AddRow("Consumable Type", c.consumableType.ToString());

        AddRow("Health Restore", c.healthAmount.ToString());
        AddRow("Stamina Restore", c.staminaAmount.ToString());
    }

    // -------------------------------------
    //  DOCUMENT INFO
    // -------------------------------------
    private void AddDocumentInfo(DocumentSO d)
    {
        AddRow("Number of pages", d.papers.Count.ToString());
    }
}