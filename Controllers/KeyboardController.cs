using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sprint0.Interfaces;

namespace Sprint0.Controllers;

public class KeyboardController : IController
{
    private readonly IPlayer player;
    private readonly Action quit;
    private readonly Action toggleCredits;
    private readonly Func<bool> isCreditsVisible;
    private KeyboardState previousState;
    private bool hadMovementInput;

    public bool AllowsMouseMovement { get; private set; }

    public KeyboardController(IPlayer player, Action quit, Action toggleCredits, Func<bool> isCreditsVisible)
    {
        this.player = player;
        this.quit = quit;
        this.toggleCredits = toggleCredits;
        this.isCreditsVisible = isCreditsVisible;
    }

    public void Update()
    {
        Update(Keyboard.GetState());
    }

    public void Update(KeyboardState currentState)
    {
        AllowsMouseMovement = false;
        if (currentState.IsKeyDown(Keys.Tab) && previousState.IsKeyUp(Keys.Tab))
        {
            player.Stop();
            toggleCredits();
        }
        else if (!isCreditsVisible())
        {
            UpdateWorldInput(currentState);
        }

        previousState = currentState;
    }

    private void UpdateWorldInput(KeyboardState currentState)
    {
        bool up = currentState.IsKeyDown(Keys.W) || currentState.IsKeyDown(Keys.Up);
        bool down = currentState.IsKeyDown(Keys.S) || currentState.IsKeyDown(Keys.Down);
        bool left = currentState.IsKeyDown(Keys.A) || currentState.IsKeyDown(Keys.Left);
        bool right = currentState.IsKeyDown(Keys.D) || currentState.IsKeyDown(Keys.Right);
        bool hasMovementInput = up || down || left || right;
        Vector2 direction = new Vector2((right ? 1 : 0) - (left ? 1 : 0),
            (down ? 1 : 0) - (up ? 1 : 0));

        if (hasMovementInput || hadMovementInput)
        {
            player.Move(direction, currentState.IsKeyDown(Keys.LeftShift));
        }
        hadMovementInput = hasMovementInput;
        AllowsMouseMovement = !hasMovementInput && !currentState.IsKeyDown(Keys.Tab)
            && !currentState.IsKeyDown(Keys.Escape);

        if (currentState.IsKeyDown(Keys.Escape) && previousState.IsKeyUp(Keys.Escape))
        {
            quit();
        }
    }
}
