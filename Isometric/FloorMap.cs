using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sprint0.Isometric;

public class FloorMap
{
    public const int GridWidth = 32;
    public const int GridHeight = 32;

    private const int SourceTileWidth = 16;
    private const float TileScale = IsometricProjection.TileWidth / (float)SourceTileWidth;

    private static readonly Rectangle lightTile = new Rectangle(0, 90, 18, 18);
    private static readonly Rectangle darkTile = new Rectangle(36, 90, 18, 18);
    private static readonly Vector2 tileOrigin = new Vector2(9, 4);

    private readonly Texture2D spriteSheet;

    public Vector2 Center => new Vector2((GridWidth - 1) / 2f, (GridHeight - 1) / 2f);

    public FloorMap(Texture2D spriteSheet)
    {
        this.spriteSheet = spriteSheet;
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 cameraOffset)
    {
        for (int y = 0; y < GridHeight; y++)
        {
            for (int x = 0; x < GridWidth; x++)
            {
                Rectangle source = (x + y) % 2 == 0 ? lightTile : darkTile;
                Vector2 screenPosition = IsometricProjection.ToScreen(new Vector2(x, y)) + cameraOffset;

                spriteBatch.Draw(spriteSheet, screenPosition, source, Color.White,
                    0f, tileOrigin, TileScale, SpriteEffects.None, 0f);
            }
        }
    }
}
