using UnityEngine;

public class CookingStation : Interactable
{
    [Header("Cooking Settings")]
    public GameObject cookingWindowPrefab;

    // Future variables (placeholders for your Zomboid system)
    [HideInInspector] public float currentFuelTime;
    [HideInInspector] public bool isLit;

    public override void OnInteract()
    {
        if (cookingWindowPrefab == null) return;

        // 1. Get the sprite from this object's SpriteRenderer
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        Sprite spriteToSend = (sr != null) ? sr.sprite : null;

        // 2. Open window
        GameObject windowGO = WindowManager.Instance.OpenWindow(cookingWindowPrefab);
        CookingStationWindow ui = windowGO.GetComponent<CookingStationWindow>();

        if (ui != null)
        {
            // 3. Pass both the station data AND the sprite
            ui.Initialize(this, spriteToSend);
        }
    }
}