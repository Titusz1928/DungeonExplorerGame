using UnityEngine;
using TMPro;

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
            AddArmorInfo(armor);

        if (instance.itemSO is ConsumableItemSO consumable)
            AddConsumableInfo(consumable);

        if (instance.itemSO is WeaponItemSO weapon)
            AddWeaponInfo(weapon);

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
    private void AddArmorInfo(ArmorItemSO armor)
    {
        AddRow("Armor Type", armor.layer.ToString());

        // If the armor covers multiple slots, join them as a string
        string covers = string.Join(", ", armor.slotsCovered);
        AddRow("Covers", covers);

        // Defense stats
        AddRow("Blunt", $"{armor.bluntRes}");
        AddRow("Pierce", $"{armor.pierceRes}");
        AddRow("Slash", $"{armor.slashRes}");

        // Stealth stats
        AddRow("Noise", $"{armor.noise}");
        AddRow("Conspicuous",$"{armor.conspicuousness}");

        // Durability / Holes
        AddRow($"Holes", $"{armor.holes}");
    }

    // -------------------------------------
    //  WEAPON INFO
    // -------------------------------------
    private void AddWeaponInfo(WeaponItemSO weapon)
    {
        AddRow("Weapon Type", weapon.weaponType.ToString());

        AddRow("Pierce Damage", weapon.pierceDamage.ToString());
        AddRow("Blunt Damage", weapon.bluntDamage.ToString());
        AddRow("Slash Damage", weapon.slashDamage.ToString());
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
}