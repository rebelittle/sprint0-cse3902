using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sprint0.Player;

namespace Sprint0.Interfaces;

public interface ISprite
{
    void Update(GameTime gameTime, Direction direction, bool isMoving, float animationSpeed = 1f);
    void Draw(SpriteBatch spriteBatch, Vector2 screenPosition);
}
