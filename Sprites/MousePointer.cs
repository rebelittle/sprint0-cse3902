using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sprint0.Sprites;

public class MousePointer
{
    private const int FrameCount = 6;
    private const double LoopDurationSeconds = 1.0;
    private readonly SpriteSheet spriteSheet;
    private double elapsedSeconds;

    public MousePointer(Texture2D texture)
    {
        spriteSheet = new SpriteSheet(texture, new Point(16, 16), Point.Zero);
    }

    public void Update(GameTime gameTime)
    {
        elapsedSeconds = (elapsedSeconds + gameTime.ElapsedGameTime.TotalSeconds)
            % LoopDurationSeconds;
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 screenPosition)
    {
        int frame = (int)(elapsedSeconds / LoopDurationSeconds * FrameCount);
        Rectangle source = spriteSheet.GetFrame(frame, 0);
        spriteBatch.Draw(spriteSheet.Texture, screenPosition, source, Color.White,
            0f, new Vector2(8f, 8f), 2f, SpriteEffects.None, 0f);
    }
}
