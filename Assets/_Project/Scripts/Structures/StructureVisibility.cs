using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureVisibility : MonoBehaviour
{
    [System.Serializable]
    public struct VisibilityZone
    {
        public Vector2 offset;
        public Vector2 size;
    }

    [Header("Component References")]
    public GameObject roofObject;
    public List<Renderer> northWalls = new();
    public List<Renderer> southWalls = new();
    public List<Renderer> westWalls = new();
    public List<Renderer> eastWalls = new();
    public List<Renderer> floors = new();
    public Renderer floor;

    public GameObject interiorBlackout;
    public GameObject insideObjects;

    [Header("Coordinate Zones (Lists)")]
    public List<VisibilityZone> insideZones = new();
    public List<VisibilityZone> northZones = new();
    public List<VisibilityZone> clearZones = new();

    private Transform player;

    public bool clearObstaclesOnStart = true;
    public LayerMask obstaclesLayerMask = ~0; // Default to 'Everything'

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        // Safety net for manually placed houses
        if (clearObstaclesOnStart)
        {
            StartCoroutine(DelayedClear());
        }
    }

    private IEnumerator DelayedClear()
    {
        yield return new WaitForEndOfFrame();
        ClearObstacles();
    }

    void Update()
    {
        if (player == null) return;

        bool isInside = false;
        bool isBehind = false;

        // 1. Check if player is in ANY Inside Zone
        foreach (var zone in insideZones)
        {
            if (IsPointInZone(player.position, zone))
            {
                isInside = true;
                break;
            }
        }

        // 2. Check if player is in ANY North Zone (Only if not inside)
        if (!isInside)
        {
            foreach (var zone in northZones)
            {
                if (IsPointInZone(player.position, zone))
                {
                    isBehind = true;
                    break;
                }
            }
        }

        // 3. Apply Logic
        if (isInside) ApplyInside();
        else if (isBehind) ApplyBehind();
        else ApplyNormal();
    }

    private bool IsPointInZone(Vector2 point, VisibilityZone zone)
    {
        Vector2 center = (Vector2)transform.position + zone.offset;
        return Mathf.Abs(point.x - center.x) < (zone.size.x / 2f) &&
               Mathf.Abs(point.y - center.y) < (zone.size.y / 2f);
    }

    public void ClearObstacles()
    {
        // Use a HashSet to ensure we don't process the same obstacle twice 
        // if clear zones overlap
        HashSet<Collider2D> uniqueHits = new HashSet<Collider2D>();

        foreach (var zone in clearZones)
        {
            Vector2 center = (Vector2)transform.position + zone.offset;
            Collider2D[] hits = Physics2D.OverlapBoxAll(center, zone.size, 0f, obstaclesLayerMask);
            foreach (var hit in hits) uniqueHits.Add(hit);
        }

        foreach (Collider2D hit in uniqueHits)
        {
            if (hit.CompareTag("Player")) continue;
            if (hit.transform.IsChildOf(this.transform)) continue;
            if (insideObjects != null && hit.transform.IsChildOf(insideObjects.transform)) continue;

            GameObject objectToDestroy = hit.transform.parent != null ? hit.transform.parent.gameObject : hit.gameObject;

            if (objectToDestroy.name.Contains("Chunk") || objectToDestroy.name.Contains("Grid")) continue;

            Vector3 obstaclePos = objectToDestroy.transform.position;
            WorldSaveData.Instance.RemoveObjectFromChunk(obstaclePos);

            Debug.Log($"[House] Clearing: {objectToDestroy.name}");
            Destroy(objectToDestroy);
        }
    }

    private void ApplyInside()
    {
        if (roofObject) SetAlpha(roofObject, 0f); // Hide roof
        if (interiorBlackout) interiorBlackout.SetActive(false);
        if (insideObjects) insideObjects.SetActive(true);

        // This will make the wall invisible, but the COLLIDER stays active!
        SetAlpha(southWalls, 0f);
        SetAlpha(northWalls, 1f);
        SetAlpha(westWalls, 0.3f);
        SetAlpha(eastWalls, 0.3f);
        SetAlpha(floors, 1f);
    }

    private void ApplyBehind()
    {
        if (interiorBlackout) interiorBlackout.SetActive(true);
        if (insideObjects) insideObjects.SetActive(false);

        SetAlpha(roofObject, 0f);
        SetAlpha(northWalls, 0.5f);
        SetAlpha(westWalls, 0f);
        SetAlpha(eastWalls, 0f);
        SetAlpha(floors, 0f);
        SetAlpha(southWalls, 1f);
    }

    private void ApplyNormal()
    {
        SetAlpha(roofObject, 1f);
        if (interiorBlackout) interiorBlackout.SetActive(true);
        if (insideObjects) insideObjects.SetActive(false);

        SetAlpha(northWalls, 1f);
        SetAlpha(southWalls, 1f);
        SetAlpha(westWalls, 1f);
        SetAlpha(eastWalls, 1f);
        SetAlpha(floors, 1f);
    }

    // --- NEW HELPER FOR LISTS ---
    private void SetAlpha(List<Renderer> rendererList, float alpha)
    {
        if (rendererList == null) return;
        foreach (Renderer r in rendererList)
        {
            SetAlpha(r, alpha);
        }
    }

    // --- LOGIC FOR GAMEOBJECTS ---
    private void SetAlpha(GameObject obj, float alpha)
    {
        if (obj == null) return;

        // If this is the ROOF or BLACKOUT, we usually want to disable the whole object
        // because they rarely have colliders that block the player.
        // BUT for walls, we only want to disable the renderer.

        // Let's check: Does this object have a Collider?
        bool hasCollider = obj.GetComponent<Collider2D>() != null;

        // If it has a collider, NEVER disable the GameObject, only the renderers.
        if (hasCollider)
        {
            // Keep object active so collider works
            if (!obj.activeSelf) obj.SetActive(true);
        }
        else
        {
            // No collider? Safe to disable fully if alpha is 0
            if (alpha <= 0)
            {
                obj.SetActive(false);
                return;
            }
            obj.SetActive(true);
        }

        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer r in renderers)
        {
            SetAlpha(r, alpha);
        }
    }

    // --- LOGIC FOR RENDERERS ---
    private void SetAlpha(Renderer renderer, float alpha)
    {
        if (renderer == null) return;

        // CRITICAL FIX: Don't disable the GameObject. Disable the Renderer.
        if (alpha <= 0)
        {
            renderer.enabled = false; // Invisible, but Object (and Collider) is still there!
            return;
        }

        renderer.enabled = true;

        if (renderer is SpriteRenderer sr)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
        else
        {
            // --- TRANSPARENCY FIX ---
            // If the alpha is < 1, we try to force the material to be transparent
            if (alpha < 1f)
            {
                Material mat = renderer.material;
                mat.SetFloat("_Surface", 1); // 1 = Transparent, 0 = Opaque (For URP Lit)
                mat.SetInt("_ZWrite", 0);
                mat.renderQueue = 3000;
            }

            Color c = renderer.material.color;
            c.a = alpha;
            renderer.material.color = c;
        }
    }

    void OnDrawGizmos()
    {
        // Draw Inside Zones
        Gizmos.color = Color.green;
        foreach (var zone in insideZones)
        {
            Vector3 center = transform.position + (Vector3)zone.offset;
            Gizmos.DrawWireCube(center, new Vector3(zone.size.x, zone.size.y, 0));
        }

        // Draw North Zones
        Gizmos.color = Color.yellow;
        foreach (var zone in northZones)
        {
            Vector3 center = transform.position + (Vector3)zone.offset;
            Gizmos.DrawWireCube(center, new Vector3(zone.size.x, zone.size.y, 0));
        }

        // Draw Clear Zones
        Gizmos.color = Color.red;
        foreach (var zone in clearZones)
        {
            Vector3 center = transform.position + (Vector3)zone.offset;
            Gizmos.DrawWireCube(center, new Vector3(zone.size.x, zone.size.y, 0));
        }
    }
}