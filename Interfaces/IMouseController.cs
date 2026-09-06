using Microsoft.Xna.Framework;

namespace Sprint0.Interfaces;

public interface IMouseController : IController
{
    Vector2 ScreenPosition { get; }
}
