using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileTerrainGenerator : MonoBehaviour
{
    [Header("Noise Settings")]
    public float noiseScale = 0.05f;
    public int seed = 12345;

    [Header("Height Thresholds")]
    public float waterLevel = 0.35f;
    public float beachLevel = 0.40f;
    public float sandLevel = 0.45f;
    public float sandyGrassLevel = 0.5f;

    [Header("Tiles")]
    public WorldTileSet tileSet;

    public IEnumerator GenerateChunkAsync(
        Tilemap tilemap,
        Vector2Int chunkCoord,
        int chunkSize
    )
    {
        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                int worldX = chunkCoord.x * chunkSize + x;
                int worldY = chunkCoord.y * chunkSize + y;

                float height = GetHeight(worldX, worldY, 1280);
                TileBase tile = GetTileFromHeight(height);

                Vector3Int cell = new Vector3Int(x, y, 0);
                tilemap.SetTile(cell, tile);

                //if (tile == tileSet.beach)
                //    ApplyBeachRotation(tilemap, cell, worldX, worldY);
            }

            yield return null;
        }
    }

    TileBase GetTileFromHeight(float h)
    {
        if (h < waterLevel) return tileSet.water;
        if (h < beachLevel) return tileSet.beach;
        if (h < sandLevel) return tileSet.sand;
        if (h < sandyGrassLevel) return tileSet.sandyGrass;
        return tileSet.grass;
    }

    ////////////////////////////////////

    public float GetHeight(int x, int y, int worldSize)
    {
        float noise = Mathf.PerlinNoise((x + seed) * noiseScale, (y + seed) * noiseScale);
        float island = GetIslandFalloff(x, y, worldSize);

        float height;

        // edges: water → beach → sand → grass
        if (island < 0.4f) // water region
        {
            height = Mathf.Lerp(0f, waterLevel, island / 0.4f);
        }
        else if (island < 0.45f) // beach region, narrow
        {
            height = Mathf.Lerp(waterLevel, beachLevel, (island - 0.4f) / 0.05f);
        }
        else if (island < 0.5f) // sand region, narrow
        {
            height = Mathf.Lerp(beachLevel, sandLevel, (island - 0.45f) / 0.05f);
        }
        else // center: grassy region
        {
            height = Mathf.Lerp(sandLevel, 1f, (island - 0.5f) / 0.5f);
        }

        // Add small noise
        height += noise * 0.03f; // smaller, just to vary tiles

        return Mathf.Clamp01(height);
    }

    float GetIslandFalloff(int x, int y, int worldSize)
    {
        Vector2 center = new Vector2(worldSize / 2f, worldSize / 2f);

        float dx = (x - center.x) / center.x;
        float dy = (y - center.y) / center.y;

        float distance = Mathf.Sqrt(dx * dx + dy * dy);

        float falloff = 1f - Mathf.Clamp01(distance);
        return falloff;
    }


    //FOR PLAYER SPAWNING

    public Vector2 FindRandomCoastlineSpawn(int worldSize, int marginPercent = 20, int maxAttempts = 10000)
    {
        int margin = worldSize * marginPercent / 100;
        for (int i = 0; i < maxAttempts; i++)
        {
            int x = Random.Range(margin, worldSize - margin);
            int y = Random.Range(margin, worldSize - margin);

            if (IsCoastline(x, y, worldSize))
            {
                Debug.Log($"Spawn found at ({x},{y})");
                return new Vector2(x + 0.5f, y + 0.5f);
            }
        }

        Debug.LogWarning("Failed to find coastline spawn, using center.");
        return new Vector2(worldSize / 2f, worldSize / 2f);
    }

    public bool IsCoastline(int x, int y, int worldSize)
    {
        float h = GetHeight(x, y, worldSize);

        // Only sand tiles allowed
        float min = 0.3f;      // 0.4
        float max = 0.6f;       // 0.5
        if (h < min || h > max)
        {
            Debug.Log($"Rejected ({x},{y}) - height {h:F3} not in sand range [{min},{max}]");
            return false;
        }

        int[] dx = { -1, 1, 0, 0, -1, -1, 1, 1 };
        int[] dy = { 0, 0, -1, 1, -1, 1, -1, 1 };


        for (int i = 0; i < dx.Length; i++)
        {
            float nh = GetHeight(x + dx[i], y + dy[i], worldSize);
            if (nh >= beachLevel && nh < sandLevel) // neighbor is beach
            {
                Debug.Log($"Accepted ({x},{y}) as coastline - neighbor ({x + dx[i]},{y + dy[i]}) is beach ({nh:F3})");
                return true;
            }
        }

        Debug.Log($"Rejected ({x},{y}) - no beach neighbors");
        return false;
    }

}
