using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace TelegramStickerMaker;

internal static class StaticStickerProcessor
{
    private const int TargetSize = 512;

    public static void Process(string inputPath, string outputPath)
    {
        using Image image = Image.Load(inputPath);

        image.Mutate(ctx => ctx.Resize(new ResizeOptions
        {
            Size = new(TargetSize, TargetSize),
            Mode = ResizeMode.Max
        }));

        image.Save(outputPath, new PngEncoder());
    }
}