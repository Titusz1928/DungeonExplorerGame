using System.Collections.Generic;


[System.Serializable]
public class PageSideInstance
{
    public byte[] inkLayer; // The player's drawings
    public string customText; // The player's writing
}

[System.Serializable]
public class PaperInstance
{
    public PageSideInstance frontSide = new PageSideInstance();
    public PageSideInstance backSide = new PageSideInstance();
}

[System.Serializable]
public class ItemInstance
{
    public ItemSO itemSO;
    public double currentDurability;
    public int quantity;
    public bool isEquipped;

    // Armor-only dynamic stat
    public int holes = 0;

    // --- Document Specific Data ---
    public List<PaperInstance> paperInstances = new List<PaperInstance>();

    public ItemInstance(ItemSO so, int quantity = 1)
    {
        itemSO = so;
        this.quantity = quantity;
        currentDurability = so.isBreakable ? so.maxDurability : 0;


        // If this is a document, we initialize the instance pages
        if (so is DocumentSO docSO)
        {
            foreach (var p in docSO.papers)
            {
                paperInstances.Add(new PaperInstance());
            }
        }
    }
}
