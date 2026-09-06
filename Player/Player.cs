using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sprint0.Interfaces;
using Sprint0.Isometric;

namespace Sprint0.Player;

public class Player : IPlayer
{
    private const float WalkSpeed = 80f;
    private const float RunSpeedMultiplier = 2f;

    private readonly ISprite sprite;
    private readonly Vector2 maximumTilePosition;
    private Vector2 movementDirection;
    private bool isRunning;
    private Vector2? targetTilePosition;
    private Direction facing = Direction.South;

    public Vector2 TilePosition { get; private set; }

    public Player(ISprite sprite, Vector2 initialTilePosition, Vector2 maximumTilePosition)
    {
        this.sprite = sprite;
        this.maximumTilePosition = maximumTilePosition;
        TilePosition = Vector2.Clamp(initialTilePosition, Vector2.Zero, maximumTilePosition);
    }

    public void Move(Vector2 screenDirection, bool isRunning = false)
    {
        targetTilePosition = null;
        SetMovement(screenDirection, isRunning);
    }

    public void MoveTo(Vector2 tilePosition)
    {
        targetTilePosition = Vector2.Clamp(tilePosition, Vector2.Zero, maximumTilePosition);
    }

    public void Stop()
    {
        Move(Vector2.Zero);
    }

    private void SetMovement(Vector2 screenDirection, bool isRunning)
    {
        movementDirection = screenDirection;
        this.isRunning = isRunning && movementDirection != Vector2.Zero;
        if (movementDirection == Vector2.Zero)
        {
            return;
        }

        movementDirection.Normalize();
        float angle = MathF.Atan2(movementDirection.X, -movementDirection.Y);
        int directionIndex = (int)MathF.Round(angle / MathHelper.PiOver4);
        facing = (Direction)((directionIndex + 8) % 8);
    }

    public void Update(GameTime gameTime)
    {
        Vector2 previousPosition = TilePosition;
        Vector2 remainingMovement = Vector2.Zero;
        if (targetTilePosition.HasValue)
        {
            remainingMovement = IsometricProjection.ToScreen(targetTilePosition.Value - TilePosition);
            SetMovement(remainingMovement, true);
        }

        float elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
        float speedMultiplier = isRunning ? RunSpeedMultiplier : 1f;
        float stepDistance = WalkSpeed * speedMultiplier * elapsedSeconds;
        if (targetTilePosition.HasValue && remainingMovement.Length() <= stepDistance)
        {
            TilePosition = targetTilePosition.Value;
            Stop();
        }
        else
        {
            Vector2 tileMovement = IsometricProjection.ToTile(movementDirection * stepDistance);
            TilePosition = Vector2.Clamp(TilePosition + tileMovement, Vector2.Zero, maximumTilePosition);
        }

        bool isMoving = TilePosition != previousPosition && movementDirection != Vector2.Zero;
        sprite.Update(gameTime, facing, isMoving, speedMultiplier);
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 screenPosition)
    {
        sprite.Draw(spriteBatch, screenPosition);
    }
}
