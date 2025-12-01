using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public static ItemSpawner Instance;

    public GameObject worldItemPrefab;

    void Awake()
    {
        Instance = this;
    }

    public void SpawnWorldItem(ItemSO item, Vector3 position, int quantity = 1)
    {
        GameObject obj = Instantiate(worldItemPrefab, position, Quaternion.identity);
        obj.GetComponent<WorldItem>().Init(item, quantity);
    }
}
