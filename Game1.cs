using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sprint0.Controllers;
using Sprint0.Interfaces;
using Sprint0.Isometric;
using Sprint0.Screens;
using Sprint0.Sprites;

namespace Sprint0;

public class Game1 : Game
{
    private const float CameraZoom = 2f;

    private readonly GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch = null!;
    private FloorMap floor = null!;
    private IPlayer player = null!;
    private IController keyboardController = null!;
    private IMouseController mouseController = null!;
    private MousePointer mousePointer = null!;
    private CreditsScreen creditsScreen = null!;
    private Texture2D pixel = null!;
    private bool creditsVisible;

    public Game1()
    {
        graphics = new GraphicsDeviceManager(this);
        graphics.PreferredBackBufferWidth = 1280;
        graphics.PreferredBackBufferHeight = 720;
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
        Window.Title = "Sprint 0 | WASD / arrows: move | Shift: run | Click: move | Tab: credits | Esc: quit";
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        Texture2D spriteSheet = Content.Load<Texture2D>("TiynBlocks_1.1/tinyBlocks_NOiL");
        floor = new FloorMap(spriteSheet);

        Texture2D characterTexture = Content.Load<Texture2D>("Characters/OrangeWalker");
        var characterSheet = new SpriteSheet(characterTexture, new Point(16, 24), new Point(0, 216));
        ISprite characterSprite = new AnimatedSprite(characterSheet, new Vector2(8, 22), 4f);
        player = new Player.Player(characterSprite, floor.Center,
            new Vector2(FloorMap.GridWidth - 1, FloorMap.GridHeight - 1));
        var keyboard = new KeyboardController(player, Exit,
            () => creditsVisible = !creditsVisible, () => creditsVisible);
        keyboardController = keyboard;
        mouseController = new MouseController(player, GetCameraTransform,
            () => GraphicsDevice.Viewport.Bounds,
            () => IsActive && !creditsVisible && keyboard.AllowsMouseMovement);
        mousePointer = new MousePointer(Content.Load<Texture2D>("Cursors/BlueCircle"));

        pixel = new Texture2D(GraphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });
        creditsScreen = new CreditsScreen(Content.Load<SpriteFont>("Fonts/Credits"), pixel);
    }

    protected override void Update(GameTime gameTime)
    {
        if (IsActive)
        {
            keyboardController.Update();
        }
        else if (!creditsVisible)
        {
            player.Stop();
        }

        mouseController.Update();
        IsMouseVisible = creditsVisible || !IsActive;

        if (!creditsVisible)
        {
            player.Update(gameTime);
            if (IsActive)
            {
                mousePointer.Update(gameTime);
            }
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(35, 39, 46));

        if (creditsVisible)
        {
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            creditsScreen.Draw(spriteBatch, GraphicsDevice.Viewport);
            spriteBatch.End();
        }
        else
        {
            DrawWorld();
        }

        base.Draw(gameTime);
    }

    private void DrawWorld()
    {
        Vector2 playerScreenPosition = IsometricProjection.ToScreen(player.TilePosition);
        spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: GetCameraTransform());
        floor.Draw(spriteBatch, Vector2.Zero);
        player.Draw(spriteBatch, playerScreenPosition);
        spriteBatch.End();

        if (IsActive && GraphicsDevice.Viewport.Bounds.Contains(mouseController.ScreenPosition.ToPoint()))
        {
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            mousePointer.Draw(spriteBatch, mouseController.ScreenPosition);
            spriteBatch.End();
        }
    }

    private Matrix GetCameraTransform()
    {
        Vector2 screenCenter = new Vector2(GraphicsDevice.Viewport.Width / 2f,
            GraphicsDevice.Viewport.Height / 2f);
        Vector2 playerScreenPosition = IsometricProjection.ToScreen(player.TilePosition);
        return Matrix.CreateTranslation(new Vector3(-playerScreenPosition, 0f))
            * Matrix.CreateScale(CameraZoom)
            * Matrix.CreateTranslation(new Vector3(screenCenter, 0f));
    }

    protected override void UnloadContent()
    {
        spriteBatch.Dispose();
        pixel.Dispose();
        base.UnloadContent();
    }
}
