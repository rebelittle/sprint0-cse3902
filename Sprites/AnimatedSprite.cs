using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sprint0.Interfaces;
using Sprint0.Player;

namespace Sprint0.Sprites;

public class AnimatedSprite : ISprite
{
    private const float PoseDurationSeconds = 0.12f;
    private const int StandingPose = 1;
    private static readonly int[] walkPoses = { 0, 1, 2, 1 };

    private readonly SpriteSheet spriteSheet;
    private readonly Vector2 origin;
    private readonly float scale;
    private Direction direction = Direction.South;
    private float walkElapsedSeconds;
    private int pose = StandingPose;

    public AnimatedSprite(SpriteSheet spriteSheet, Vector2 origin, float scale)
    {
        this.spriteSheet = spriteSheet;
        this.origin = origin;
        this.scale = scale;
    }

    public void Update(GameTime gameTime, Direction direction, bool isMoving, float animationSpeed = 1f)
    {
        this.direction = direction;

        if (isMoving)
        {
            walkElapsedSeconds = (walkElapsedSeconds
                + (float)gameTime.ElapsedGameTime.TotalSeconds * animationSpeed)
                % (PoseDurationSeconds * walkPoses.Length);
            pose = walkPoses[(int)(walkElapsedSeconds / PoseDurationSeconds)];
        }
        else
        {
            walkElapsedSeconds = 0f;
            pose = StandingPose;
        }
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 screenPosition)
    {
        Rectangle source = spriteSheet.GetFrame((int)direction, pose);
        spriteBatch.Draw(spriteSheet.Texture, screenPosition, source, Color.White,
            0f, origin, scale, SpriteEffects.None, 0f);
    }
}
