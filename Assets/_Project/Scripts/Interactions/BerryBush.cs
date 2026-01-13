using UnityEngine;

public class BerryBush : Interactable
{
    [Header("Harvest Settings")]
    public int berryItemID = 8;
    public int berryCount = 3;
    public GameObject emptyBushPrefab; // The "A" variant

    public override void OnInteract()
    {
        Debug.Log($"[HARVEST] Collected {berryCount} berries.");

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Replace 'Inventory' with your actual inventory script name
            Inventory playerInventory = player.GetComponent<Inventory>();
            if (playerInventory != null)
            {
                playerInventory.AddItem(ItemDatabase.instance.GetByID(berryItemID), berryCount);
            }
        }

        // 2. TELL THE SAVE SYSTEM THE STATE CHANGED
        // We pass the name of the 'A' variant (empty bush) prefab
        if (emptyBushPrefab != null)
        {
            WorldSaveData.Instance.UpdateObjectPrefabInChunk(transform.position, emptyBushPrefab.name);

            // 3. Spawn the 'A' Variant
            Instantiate(emptyBushPrefab, transform.position, transform.rotation, transform.parent);
        }

        // 3. Destroy this "B" Variant
        Destroy(gameObject);
    }
}