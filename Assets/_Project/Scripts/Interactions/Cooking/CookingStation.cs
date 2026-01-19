using UnityEngine;

public class CookingStation : Interactable
{
    [Header("Cooking Settings")]
    public GameObject cookingWindowPrefab;

    private WorldContainer container;

    [Header("Station State")]
    public bool isOnFire;
    public float remainingFuelTime;
    public float maxFuelCapacity = 720f;


    public bool inventoryUpdated = false;


    private SpriteRenderer worldSpriteRenderer;
    public Sprite StationNotOnFireSprite;
    public Sprite StationOnFireSprite;

    [Header("Visual Effects")]
    public ParticleSystem fireParticles;

    protected override void Start()
    {
        base.Start();
        container = GetComponent<WorldContainer>();

        // Cache the renderer so we don't look it up every frame
        worldSpriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // Set initial sprite
        UpdateWorldVisuals();
    }

    protected override void Update()
    {
        base.Update();

        if (isOnFire)
        {
            HandleBurning();
            HandleCooking();
        }
    }

    // Logic to swap the sprite and toggle particles based on state
    public void UpdateWorldVisuals()
    {
        // 1. Handle Sprite Swap
        if (worldSpriteRenderer != null)
        {
            worldSpriteRenderer.sprite = isOnFire ? StationOnFireSprite : StationNotOnFireSprite;
        }

        // 2. Handle Particles
        if (fireParticles != null)
        {
            if (isOnFire)
            {
                // Play particles if they aren't already playing
                if (!fireParticles.isPlaying) fireParticles.Play();
            }
            else
            {
                // Stop particles
                fireParticles.Stop();
            }
        }
    }

    private void HandleBurning()
    {
        if (remainingFuelTime > 0)
        {
            remainingFuelTime -= Time.deltaTime;
        }
        else
        {
            if (!TryConsumeFuel())
            {
                // Set the state FIRST
                isOnFire = false;
                // Then update visuals so it sees 'false'
                UpdateWorldVisuals();
                Debug.Log("Fire went out!");
            }
        }
    }

    private bool TryConsumeFuel()
    {
        // Look for the first fuel item in the container
        for (int i = 0; i < container.items.Count; i++)
        {
            ItemInstance item = container.items[i];
            if (item.itemSO.isFuel)
            {
                remainingFuelTime += item.itemSO.fuelValue;

                // Reduce quantity or remove item
                if (item.quantity > 1) item.quantity--;
                else container.items.RemoveAt(i);


                inventoryUpdated = true;
                return true;
            }
        }
        return false;
    }

    // This calculates how much fuel is SITTING in the container right now
    public float GetTotalPotentialFuel()
    {
        float total = remainingFuelTime;
        if (container == null || container.items == null) return total;

        foreach (var item in container.items)
        {
            if (item.itemSO != null && item.itemSO.isFuel)
            {
                total += (item.itemSO.fuelValue * item.quantity);
            }
        }
        return total;
    }



    private void HandleCooking()
    {
        // We iterate backwards so we can safely remove/replace items
        for (int i = container.items.Count - 1; i >= 0; i--)
        {
            ItemInstance item = container.items[i];

            if (item.itemSO.isCookable)
            {
                item.cookingProgress += Time.deltaTime;

                if (item.cookingProgress >= item.itemSO.cookTimeRequired)
                {
                    TransformItem(i, item);
                }
            }
        }
    }

    private void TransformItem(int index, ItemInstance oldItem)
    {
        ItemSO resultSO = oldItem.itemSO.cookedResultSO;

        if (resultSO != null)
        {
            // Create the new COOKED item
            // We keep the quantity (e.g. 5 Raw Meat -> 5 Cooked Meat)
            ItemInstance cookedItem = new ItemInstance(resultSO, oldItem.quantity);

            // Replace the old item in the list
            container.items[index] = cookedItem;

            container.SaveState();

            // MARK DIRTY: Tell the UI to redraw because "Raw Meat" is gone
            inventoryUpdated = true;

            Debug.Log($"[Cooking] {oldItem.itemSO.itemName} cooked into {resultSO.itemName}");
        }
    }

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