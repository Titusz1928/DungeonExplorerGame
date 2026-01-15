using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseVisibility : MonoBehaviour
{
    [Header("Component References")]
    public GameObject roofObject;
    public Renderer northWall;
    public Renderer southWall;
    public Renderer westWall;
    public Renderer eastWall;
    public Renderer floor;
    public GameObject interiorBlackout;
    public GameObject insideObjects;

    [Header("Coordinate Zones")]
    public Vector2 floorOffset;      // Offset for the interior/green box
    public Vector2 northZoneOffset;  // Offset for the behind/yellow box
    public float width = 6f;
    public float height = 2f;
    public float northZoneHeight = 3f;

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

        // 1. Calculate relative positions for BOTH zones independently
        Vector3 insideCenter = transform.position + (Vector3)floorOffset;
        Vector3 relInside = player.position - insideCenter;

        Vector3 behindCenter = transform.position + (Vector3)northZoneOffset;
        Vector3 relBehind = player.position - behindCenter;

        // 2. Zone Detection
        // Is player inside the Green Box?
        bool isInside = Mathf.Abs(relInside.x) < (width / 2) &&
                        Mathf.Abs(relInside.y) < (height / 2);

        // Is player inside the Yellow Box?
        bool isBehind = Mathf.Abs(relBehind.x) < (width / 2) &&
                        Mathf.Abs(relBehind.y) < (northZoneHeight / 2);

        // 3. Apply Logic
        if (isInside) ApplyInside();
        else if (isBehind) ApplyBehind();
        else ApplyNormal();
    }

    public void ClearObstacles()
    {
        Vector2 center = (Vector2)transform.position + floorOffset;
        Vector2 size = new Vector2(width, height);

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f, obstaclesLayerMask);

        foreach (Collider2D hit in hits)
        {
            // 1. Is it the player? Skip.
            if (hit.CompareTag("Player")) continue;

            // 2. Is it part of THIS house (walls, floor, roof)? Skip.
            if (hit.transform.IsChildOf(this.transform)) continue;

            // 3. Is it specifically inside the "Inside Objects" folder? Skip.
            if (insideObjects != null && hit.transform.IsChildOf(insideObjects.transform)) continue;

            // 4. If we got here, it's a stray tree/rock. Kill it.
            Debug.Log($"[House] Clearing intersecting object: {hit.name}");
            Destroy(hit.gameObject);
        }
    }

    private void ApplyInside()
    {
        if (roofObject) SetAlpha(roofObject, 0f); // Hide roof
        if (interiorBlackout) interiorBlackout.SetActive(false);
        if (insideObjects) insideObjects.SetActive(true);

        // This will make the wall invisible, but the COLLIDER stays active!
        if (southWall) SetAlpha(southWall, 0f);

        SetAlpha(northWall, 1f);
        SetAlpha(westWall, 0.3f);
        SetAlpha(eastWall, 0.3f);
        SetAlpha(floor, 1f);
    }

    private void ApplyBehind()
    {
        if (interiorBlackout) interiorBlackout.SetActive(true);
        if (insideObjects) insideObjects.SetActive(false);

        SetAlpha(roofObject, 0f);
        SetAlpha(northWall, 0.5f);

        SetAlpha(westWall, 0f);
        SetAlpha(eastWall, 0f);
        SetAlpha(floor, 0f);
        SetAlpha(southWall, 1f);
    }

    private void ApplyNormal()
    {
        SetAlpha(roofObject, 1f);
        if (interiorBlackout) interiorBlackout.SetActive(true);
        if (insideObjects) insideObjects.SetActive(false);

        SetAlpha(northWall, 1f);
        SetAlpha(southWall, 1f);
        SetAlpha(westWall, 1f);
        SetAlpha(eastWall, 1f);
        SetAlpha(floor, 1f);
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
        // GREEN BOX: Floor Area
        Vector3 insideCenter = transform.position + (Vector3)floorOffset;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(insideCenter, new Vector3(width, height, 0));

        // YELLOW BOX: Detection Area behind the house
        Vector3 behindCenter = transform.position + (Vector3)northZoneOffset;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(behindCenter, new Vector3(width, northZoneHeight, 0));
    }
}