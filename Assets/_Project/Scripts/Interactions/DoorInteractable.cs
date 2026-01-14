using UnityEngine;

public class DoorInteractable : Interactable
{
    [Header("Door Settings")]
    [Tooltip("The prefab this door turns into when interacted with.")]
    public GameObject otherDoorPrefab;

    [Header("Placement Offset")]
    [Tooltip("X: 0.35, Y: -0.2 for A->B | X: -0.35, Y: 0.2 for B->A")]
    public Vector2 spawnOffset;

    public bool isLocked = false;
    public int requiredKeyID = 99;

    public override void OnInteract()
    {
        if (isLocked)
        {
            Debug.Log("[DOOR] The door is locked.");
            return;
        }

        if (otherDoorPrefab == null) return;

        // 1. Calculate the New Position
        Vector3 newPos = transform.position + new Vector3(spawnOffset.x, spawnOffset.y, 0);

        // 2. UPDATE SAVE SYSTEM
        // We save the NEW position so it loads correctly in the future
        if (WorldSaveData.Instance != null)
        {
            WorldSaveData.Instance.UpdateObjectPrefabInChunk(newPos, otherDoorPrefab.name);
        }

        // 3. SPAWN THE NEW DOOR
        GameObject newDoor = Instantiate(otherDoorPrefab, newPos, transform.rotation, transform.parent);

        // 4. DESTROY THE OLD DOOR
        Destroy(gameObject);

        Debug.Log($"[DOOR] Swapped. Offset Applied: {spawnOffset}");
    }
}