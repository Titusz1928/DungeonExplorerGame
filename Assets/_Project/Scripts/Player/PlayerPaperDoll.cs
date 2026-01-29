using System.Collections.Generic;
using UnityEngine;

public class PlayerPaperDoll : MonoBehaviour
{
    public SpriteRenderer bodyRenderer;

    // Map a unique Key (Slot + Layer) to a Sprite Array
    // e.g. "Legs_Under" -> [32 Sprites]
    private Dictionary<string, Sprite[]> activeVisuals = new Dictionary<string, Sprite[]>();

    // Map that same Key to its specific SpriteRenderer child
    private Dictionary<string, SpriteRenderer> renderers = new Dictionary<string, SpriteRenderer>();

    private void Awake()
    {
        InitializeRenderers();
    }

    public void InitializeRenderers()
    {
        // Clear to prevent double-entries
        renderers.Clear();

        // This finds all SpriteRenderers in children, even if nested
        SpriteRenderer[] childRenderers = GetComponentsInChildren<SpriteRenderer>(true);

        foreach (var sr in childRenderers)
        {
            // Skip the main body
            if (sr == bodyRenderer) continue;

            // Check if naming convention is correct (e.g., Legs_Under)
            if (sr.name.Contains("_"))
            {
                renderers[sr.name] = sr;
                Debug.Log($"[PaperDoll] Registered Renderer: {sr.name}");
            }
        }
    }

    public void SetSlotVisual(ArmorSlot slot, ArmorLayer layer, Sprite[] frames)
    {
        string key = $"{slot}_{layer}";
        activeVisuals[key] = frames;

        bool hasRenderer = renderers.ContainsKey(key);
        Debug.Log($"[PaperDoll] Visual set for {key}. Found Renderer: {hasRenderer}. Frames: {frames?.Length}");
    }

    private void LateUpdate()
    {
        if (bodyRenderer.sprite == null) return;

        int index = ParseSpriteIndex(bodyRenderer.sprite.name);

        // Log the index once per second so we don't spam the console too hard
        if (Time.frameCount % 600 == 0)
            Debug.Log($"[PaperDoll] LateUpdate: Body Sprite={bodyRenderer.sprite.name}, Parsed Index={index}");

        foreach (var entry in activeVisuals)
        {
            if (renderers.TryGetValue(entry.Key, out SpriteRenderer sr))
            {
                if (entry.Value != null && index < entry.Value.Length)
                {
                    sr.sprite = entry.Value[index];
                }
                else
                {
                    // This would clear the sprite if the index is out of bounds
                    sr.sprite = null;
                }
            }
        }
    }

    private int ParseSpriteIndex(string spriteName)
    {
        // Looks for the number after the last underscore (e.g., "Player_Walk_14" -> 14)
        string[] parts = spriteName.Split('_');
        if (parts.Length > 0 && int.TryParse(parts[parts.Length - 1], out int index))
        {
            return index;
        }
        return 0;
    }

    public void ClearSlotVisual(ArmorSlot slot, ArmorLayer layer)
    {
        string key = $"{slot}_{layer}";
        if (activeVisuals.ContainsKey(key))
        {
            activeVisuals[key] = null; // Stops the sync logic
        }

        if (renderers.TryGetValue(key, out SpriteRenderer sr))
        {
            sr.sprite = null; // Clears the actual image
        }
    }

    public void SetItemVisual(string slotName, Sprite[] frames)
    {
        // slotName would be "Weapon" or "Shield" or "Torch"
        // This matches GameObjects named "Weapon_Main", etc.
        activeVisuals[slotName] = frames;

        if (!renderers.ContainsKey(slotName))
        {
            Debug.LogWarning($"[PaperDoll] No renderer found for Item Slot: {slotName}");
        }
    }

    public void ClearItemVisual(string slotName)
    {
        activeVisuals[slotName] = null;
        if (renderers.TryGetValue(slotName, out SpriteRenderer sr))
        {
            sr.sprite = null;
        }
    }
}