using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sprint0.Sprites;

public class SpriteSheet
{
    private readonly Point frameSize;
    private readonly Point offset;

    public Texture2D Texture { get; }

    public SpriteSheet(Texture2D texture, Point frameSize, Point offset)
    {
        Texture = texture;
        this.frameSize = frameSize;
        this.offset = offset;
    }

    public Rectangle GetFrame(int column, int row)
    {
        return new Rectangle(offset.X + column * frameSize.X,
            offset.Y + row * frameSize.Y, frameSize.X, frameSize.Y);
    }
}
