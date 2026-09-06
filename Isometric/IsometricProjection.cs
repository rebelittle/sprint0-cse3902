using Microsoft.Xna.Framework;

namespace Sprint0.Isometric;

public static class IsometricProjection
{
    public const int TileWidth = 64;
    public const int TileHeight = 32;

    private const float HalfTileWidth = TileWidth / 2f;
    private const float HalfTileHeight = TileHeight / 2f;

    public static Vector2 ToScreen(Vector2 tilePosition)
    {
        return new Vector2(
            (tilePosition.X - tilePosition.Y) * HalfTileWidth,
            (tilePosition.X + tilePosition.Y) * HalfTileHeight);
    }

    public static Vector2 ToTile(Vector2 screenPosition)
    {
        float horizontal = screenPosition.X / HalfTileWidth;
        float vertical = screenPosition.Y / HalfTileHeight;

        return new Vector2((horizontal + vertical) / 2f, (vertical - horizontal) / 2f);
    }
}
