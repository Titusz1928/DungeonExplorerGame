using UnityEngine;

public class WorldItem : MonoBehaviour
{
    public ItemSO itemData;
    public int quantity = 1;

    public void Init(ItemSO data, int qty = 1)
    {
        itemData = data;
        quantity = qty;

        var sr = GetComponent<SpriteRenderer>();
        sr.sprite = data.icon;
    }
}
