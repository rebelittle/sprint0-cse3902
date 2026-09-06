using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sprint0.Screens;

public class CreditsScreen
{
    private const int Margin = 40;
    private const int Padding = 24;
    private const int LineGap = 12;
    private readonly SpriteFont font;
    private readonly Texture2D pixel;
    private readonly string[] credits =
    {
        "Program Made By: Reagan Little",
        "Sprites from: Itch.io",
        "axulart for the character and dani-maccari for the tiles"
    };

    public CreditsScreen(SpriteFont font, Texture2D pixel)
    {
        this.font = font;
        this.pixel = pixel;
    }

    public void Draw(SpriteBatch spriteBatch, Viewport viewport)
    {
        Color accent = new Color(239, 157, 76);
        spriteBatch.Draw(pixel, new Rectangle(0, 0, viewport.Width, viewport.Height),
            new Color(26, 30, 37));
        spriteBatch.DrawString(font, "Credits", new Vector2(Margin, Margin), accent,
            0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
        spriteBatch.DrawString(font, "Tab: return to world",
            new Vector2(Margin, Margin + font.LineSpacing * 2 + LineGap),
            new Color(190, 196, 204));

        int boxHeight = Padding * 2 + font.LineSpacing * credits.Length
            + LineGap * (credits.Length - 1);
        var box = new Rectangle(Margin, viewport.Height - Margin - boxHeight,
            viewport.Width - Margin * 2, boxHeight);
        spriteBatch.Draw(pixel, box, accent);
        spriteBatch.Draw(pixel, new Rectangle(box.X + 2, box.Y + 2,
            box.Width - 4, box.Height - 4), new Color(42, 47, 57));

        for (int line = 0; line < credits.Length; line++)
        {
            var position = new Vector2(box.X + Padding,
                box.Y + Padding + line * (font.LineSpacing + LineGap));
            spriteBatch.DrawString(font, credits[line], position, Color.White);
        }
    }
}
