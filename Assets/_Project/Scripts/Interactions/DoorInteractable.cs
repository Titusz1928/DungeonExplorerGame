using UnityEngine;
using UnityEngine.UI;

public class DoorInteractable : Interactable
{
    [Header("Door Settings")]
    [Tooltip("The prefab this door turns into when interacted with.")]
    public GameObject otherDoorPrefab;

    [Header("Placement Offset")]
    [Tooltip("X: 0.35, Y: -0.2 for A->B | X: -0.35, Y: 0.2 for B->A")]
    public Vector2 spawnOffset;

    public bool isLocked = false;
    [SerializeField] private int requiredKeyID = 99;

    [SerializeField] private Sprite lockedSprite;

    [SerializeField] private Image buttonImage;

    protected override void Start()
    {
        base.Start();
        // If you want a different sprite when locked vs unlocked
        if (isLocked && lockedSprite != null)
        {
            buttonImage.sprite = lockedSprite;
        }
    }

    public override void OnInteract()
    {
        if (isLocked)
        {
            // Use your static reference to get the Player's Inventory component
            Inventory playerInv = GameBoot.PersistentPlayer.GetComponent<Inventory>();

            // Find the key instance
            ItemInstance keyFound = playerInv.items.Find(i => i.itemSO.ID == requiredKeyID);

            if (keyFound != null)
            {
                Debug.Log($"[DOOR] Key {keyFound.itemSO.itemName} found. Unlocking...");
                UnlockAndOpen(keyFound);
            }
            else
            {
                Debug.Log($"[DOOR] Locked. Needs Key ID: {requiredKeyID}");
            }
        }
        else
        {
            OpenDoor();
        }
    }

    private void UnlockAndOpen(ItemInstance key)
    {
        // Check if it's a KeyItemSO to play its specific sound
        if (key.itemSO is KeyItemSO keySO && keySO.unlockSound != null)
        {
            AudioSource.PlayClipAtPoint(keySO.unlockSound, transform.position);
        }

        isLocked = false;
        OpenDoor();
    }

    private void OpenDoor()
    {
        if (otherDoorPrefab == null) return;

        Vector3 newPos = transform.position + new Vector3(spawnOffset.x, spawnOffset.y, 0);

        // Update Save System so this chunk remembers the door is now the 'Open' version
        if (WorldSaveData.Instance != null)
        {
            WorldSaveData.Instance.UpdateObjectPrefabInChunk(newPos, otherDoorPrefab.name);
        }

        Instantiate(otherDoorPrefab, newPos, transform.rotation, transform.parent);
        Destroy(gameObject);
    }
}