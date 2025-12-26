using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public static ItemSpawner Instance;

    public GameObject worldItemPrefab;

    void Awake()
    {
        Instance = this;
    }

    public void SpawnWorldItem(ItemInstance instance, Vector2 position)
    {
        GameObject obj = Instantiate(worldItemPrefab, position, Quaternion.identity);
        WorldItem wi = obj.GetComponent<WorldItem>();

        // Use the data from the actual instance
        wi.Initialize(instance);

        // Update the visual icon
        obj.GetComponent<SpriteRenderer>().sprite = instance.itemSO.icon;
    }
}
