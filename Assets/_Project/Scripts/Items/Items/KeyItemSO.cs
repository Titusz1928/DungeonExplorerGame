using UnityEngine;



[CreateAssetMenu(fileName = "NewKey", menuName = "Inventory/Key Item")]
public class KeyItemSO : ItemSO
{
    [Header("Key Properties")]
    public string doorDescription; // e.g., "An old brass key with a 'Library' tag."
    public AudioClip unlockSound;

    private void OnEnable()
    {
        category = ItemCategory.Key;
        isStackable = false; // Usually keys shouldn't stack
    }
}