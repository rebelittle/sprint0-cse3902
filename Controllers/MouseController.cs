using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sprint0.Interfaces;
using Sprint0.Isometric;

namespace Sprint0.Controllers;

public class MouseController : IMouseController
{
    private readonly IPlayer player;
    private readonly Func<Matrix> getCameraTransform;
    private readonly Func<Rectangle> getWindowBounds;
    private readonly Func<bool> isInputEnabled;
    private MouseState previousState;

    public Vector2 ScreenPosition { get; private set; }

    public MouseController(IPlayer player, Func<Matrix> getCameraTransform,
        Func<Rectangle> getWindowBounds, Func<bool> isInputEnabled)
    {
        this.player = player;
        this.getCameraTransform = getCameraTransform;
        this.getWindowBounds = getWindowBounds;
        this.isInputEnabled = isInputEnabled;
    }

    public void Update()
    {
        Update(Mouse.GetState());
    }

    public void Update(MouseState currentState)
    {
        ScreenPosition = currentState.Position.ToVector2();
        bool clicked = currentState.LeftButton == ButtonState.Pressed
            && previousState.LeftButton == ButtonState.Released;

        if (clicked && isInputEnabled() && getWindowBounds().Contains(currentState.Position))
        {
            Matrix inverseCamera = Matrix.Invert(getCameraTransform());
            Vector2 worldPosition = Vector2.Transform(ScreenPosition, inverseCamera);
            player.MoveTo(IsometricProjection.ToTile(worldPosition));
        }

        previousState = currentState;
    }
}
