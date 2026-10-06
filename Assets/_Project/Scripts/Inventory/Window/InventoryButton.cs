using TitusGames.Framework;
using UnityEngine;
using UnityEngine.UI;

public class InventoryButton : MonoBehaviour
{
    private Button button;
    private InventoryWindowInitializer invInitializer;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (ServiceLocator.Current.TryGet<IUIManagerService>(out var uiManagerService))
        {
            invInitializer = uiManagerService.GameObject.GetComponent<InventoryWindowInitializer>();
        }

        if (button != null && invInitializer != null)
        {
            button.onClick.AddListener(invInitializer.OpenInventoryWindow);
        }
        else
        {
            Debug.LogWarning($"[InventoryButton] Failed to bind button on {gameObject.name}. " +
                             $"Button null: {button == null}, Initializer null: {invInitializer == null}");
        }
    }
}
