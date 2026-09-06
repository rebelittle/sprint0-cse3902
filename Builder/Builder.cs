using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using MonoGame.Framework.Content.Pipeline.Builder;

var builder = new Builder();
builder.Run(args);
return builder.FailedToBuild > 0 ? 1 : 0;

public class Builder : ContentBuilder
{
    public override IContentCollection GetContentCollection()
    {
        var content = new ContentCollection();
        var textureProcessor = new TextureProcessor
        {
            ColorKeyEnabled = false,
            GenerateMipmaps = false,
            PremultiplyAlpha = true,
            TextureFormat = TextureProcessorOutputFormat.Color
        };

        content.Include("TiynBlocks_1.1/tinyBlocks_NOiL.png", new TextureImporter(), textureProcessor);
        content.Include("Small-8-Direction-Characters_by_AxulArt/"
            + "Small-8-Direction-Characters_by_AxulArt/Small-8-Direction-Characters_by_AxulArt.png",
            "Characters/OrangeWalker", new TextureImporter(), textureProcessor);
        content.Include("cursors/light/busy.png", "Cursors/BlueCircle",
            new TextureImporter(), textureProcessor);
        content.Include("Fonts/Credits.spritefont", new FontDescriptionImporter(),
            new FontDescriptionProcessor());
        return content;
    }
}
