using UnityEngine;
using System.Collections.Generic;

public class HouseVisibility : MonoBehaviour
{
    [Header("Component References")]
    public GameObject roofObject;
    public SpriteRenderer northWall;
    public SpriteRenderer southWall;
    public SpriteRenderer westWall;
    public SpriteRenderer eastWall;
    public SpriteRenderer floor;
    public GameObject interiorBlackout;
    public GameObject insideObjects;

    [Header("Coordinate Zones")]
    public Vector2 zoneOffset; // <--- ADDED THIS: Move the boxes from the Inspector
    public float width = 6f;
    public float height = 2f;
    public float northZoneHeight = 3f;

    private Transform player;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        // 1. Calculate relative position INCLUDING the offset
        // We subtract the offset from the relative position to "shift" the detection
        Vector3 centerPosition = transform.position + (Vector3)zoneOffset;
        Vector3 rel = player.position - centerPosition;

        // 2. Zone Detection (Math remains the same, but 'rel' is now offset)
        bool isInside = Mathf.Abs(rel.x) < (width / 2) && Mathf.Abs(rel.y) < (height / 2);

        bool isBehind = Mathf.Abs(rel.x) < (width / 2) &&
                        (rel.y >= height / 2 && rel.y < (height / 2 + northZoneHeight));

        // 3. Apply Logic
        if (isInside)
        {
            ApplyInside();
        }
        else if (isBehind)
        {
            ApplyBehind();
        }
        else
        {
            ApplyNormal();
        }
    }

    private void ApplyInside()
    {
        if (roofObject) roofObject.SetActive(false);
        if (interiorBlackout) interiorBlackout.SetActive(false);
        SetWallAlpha(southWall, 0f);
        SetWallAlpha(northWall, 1f);
        SetWallAlpha(westWall, 0.3f); // Semi-transparent sides while inside
        insideObjects.gameObject.SetActive(true);
        SetWallAlpha(floor, 1f);
        SetWallAlpha(eastWall, 0.3f);
    }

    private void ApplyBehind()
    {
        if (roofObject) roofObject.SetActive(false);
        if (interiorBlackout) interiorBlackout.SetActive(true);
        SetWallAlpha(northWall, 0f);
        SetWallAlpha(westWall, 0f);
        SetWallAlpha(eastWall, 0f);
        SetWallAlpha(floor, 0f);
        insideObjects.gameObject.SetActive(false);
        SetWallAlpha(southWall, 1f);
    }

    private void ApplyNormal()
    {
        if (roofObject) roofObject.SetActive(true);
        if (interiorBlackout) interiorBlackout.SetActive(true);
        SetWallAlpha(northWall, 1f);
        SetWallAlpha(southWall, 1f);
        SetWallAlpha(westWall, 1f); // Reset side walls to solid
        SetWallAlpha(eastWall, 1f);
    }

    private void SetWallAlpha(SpriteRenderer renderer, float alpha)
    {
        if (renderer == null) return;
        Color c = renderer.color;
        c.a = alpha;
        renderer.color = c;
    }

    // Update Gizmos to show the offset boxes
    void OnDrawGizmos()
    {
        Vector3 center = transform.position + (Vector3)zoneOffset;

        // GREEN BOX: Floor Area
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, new Vector3(width, height, 0));

        // YELLOW BOX: Detection Area behind the house
        Gizmos.color = Color.yellow;
        Vector3 northZoneCenter = center + new Vector3(0, (height / 2) + (northZoneHeight / 2), 0);
        Gizmos.DrawWireCube(northZoneCenter, new Vector3(width, northZoneHeight, 0));
    }
}