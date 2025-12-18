using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "World/Tile Set")]
public class WorldTileSet : ScriptableObject
{
    public TileBase water;
    public TileBase beach;
    public TileBase sand;
    public TileBase sandyGrass;
    public TileBase grass;
}
