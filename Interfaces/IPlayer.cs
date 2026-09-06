using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sprint0.Interfaces;

public interface IPlayer
{
    Vector2 TilePosition { get; }
    void Move(Vector2 screenDirection, bool isRunning = false);
    void MoveTo(Vector2 tilePosition);
    void Stop();
    void Update(GameTime gameTime);
    void Draw(SpriteBatch spriteBatch, Vector2 screenPosition);
}
