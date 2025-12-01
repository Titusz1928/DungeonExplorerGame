using UnityEngine;
using UnityEngine.UI;

public class InventoryButton : MonoBehaviour
{
    private Button button;
    private InventoryWindowInitializer invInitializer;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (UIManager.Instance != null)
        {
            invInitializer = UIManager.Instance.GetComponent<InventoryWindowInitializer>();
        }

        if (button != null && invInitializer != null)
        {
            button.onClick.AddListener(invInitializer.OpenInventoryWindow);
        }
    }
}
