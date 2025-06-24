using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using LucidStandardImport.model;

namespace LucidStandardImport.util;

public static class ImageSharpHelper
{
    /// <summary>
    /// Loads a PNG from <paramref name="inputPngBytes"/>, optionally rotates it,
    /// optionally converts it to grayscale, then resizes it to fit within
    /// the <paramref name="fitToBox"/> (preserving aspect ratio), and finally exports
    /// either:
    ///  • a true 8-bit grayscale PNG, or
    ///  • an 8-bit indexed (palette) PNG in full color (Wu quantization).
    ///
    /// Everything happens in memory; returns a new byte[] with the compressed PNG.
    /// </summary>
    /// <param name="inputPngBytes">
    ///   Original PNG data. Must be a valid PNG byte[].
    /// </param>
    /// <param name="fitToBox">
    ///   A BoundingBox indicating the final width (W) and height (H).
    ///   Rotation (in degrees, clockwise) is applied first (if non-null).
    ///   X and Y are unused here but included if you later want to do cropping offsets.
    /// </param>
    /// <param name="exportGrayscale">
    ///   If true → convert to true 8-bit grayscale (no colors).
    ///   If false → keep in color, but apply palette quantization (WuQuantizer).
    /// </param>
    /// <param name="maxColors">
    ///   Only used if <paramref name="exportGrayscale"/> == false.
    ///   This is the maximum number of colors in the final indexed palette (2‒256).
    /// </param>
    /// <param name="compressionLevel">
    ///   zlib compression level (0-9). Level9 = best (slowest), Level0 = fastest (least).
    /// </param>
    /// <returns>
    ///   A byte[] containing the newly encoded PNG that fits within box.W×box.H,
    ///   optionally rotated/grayscale.
    /// </returns>
    public static async Task<Image> ProcessPngAsync(
        Image image,
        BoundingBox fitToBox = null,
        bool exportGrayscale = false,
        int maxColors = 256,
        PngCompressionLevel compressionLevel = PngCompressionLevel.Level9
    )
    {
        if (image == null)
            throw new ArgumentNullException(nameof(image), "Image cannot be null.");

        if (fitToBox == null)
            throw new ArgumentNullException(nameof(fitToBox), "BoundingBox must be provided.");

        if (!exportGrayscale)
        {
            if (maxColors < 2 || maxColors > 256)
                throw new ArgumentException(
                    "maxColors must be between 2 and 256 for color output.",
                    nameof(maxColors)
                );
        }

        if (exportGrayscale)
        {
            image.Mutate(ctx => ctx.Grayscale());
        }

        if (fitToBox != null && fitToBox.W > 0 && fitToBox.H > 0)
        {
            var resizeOptions = new ResizeOptions
            {
                Size = new Size((int)Math.Round(fitToBox.W), (int)Math.Round(fitToBox.H)),
                Mode = ResizeMode.Max,
                Sampler = KnownResamplers.Lanczos3,
                Position = AnchorPositionMode.Center
            };
            image.Mutate(ctx => ctx.Resize(resizeOptions));
        }

        var pngEncoder = new PngEncoder { CompressionLevel = compressionLevel };

        if (exportGrayscale)
        {
            pngEncoder.ColorType = PngColorType.Grayscale;
        }
        else
        {
            pngEncoder.ColorType = PngColorType.Palette;
            pngEncoder.Quantizer = new WuQuantizer(
                new QuantizerOptions { MaxColors = maxColors, Dither = null }
            );
        }

        using var msOut = new MemoryStream();
        await image.SaveAsync(msOut, pngEncoder);
        msOut.Position = 0;

        var compressedImage = await Image.LoadAsync<Rgba32>(msOut);
        return compressedImage;
    }

    private const int MaxTileSizePx = 1000;

    public static List<ImageShape> TileImage(
        this ImageShape sourceShape,
        Image sourceImage,
        int? maxPx = MaxTileSizePx
    )
    {
        if (sourceImage == null)
            throw new ArgumentNullException(nameof(sourceImage), "Source image cannot be null.");
        if (maxPx <= 0)
            throw new ArgumentException("maxPx must be greater than 0.");

        if (Math.Max(sourceImage.Width, sourceImage.Height) < maxPx)
            return [sourceShape];

        int horizontalTiles = (int)Math.Ceiling(sourceImage.Width / (double)maxPx);
        int verticalTiles = (int)Math.Ceiling(sourceImage.Height / (double)maxPx);

        var tiles = new List<ImageShape>();

        var originX = sourceShape.BoundingBox?.X ?? 0;
        var originY = sourceShape.BoundingBox?.Y ?? 0;

        for (int y = 0; y < verticalTiles; y++)
        {
            for (int x = 0; x < horizontalTiles; x++)
            {
                int left = x * maxPx.Value;
                int top = y * maxPx.Value;
                int width = Math.Min(maxPx.Value, sourceImage.Width - left);
                int height = Math.Min(maxPx.Value, sourceImage.Height - top);

                if (width <= 0 || height <= 0)
                    continue;

                var tile = sourceImage.Clone(
                    ctx => ctx.Crop(new Rectangle(left, top, width, height))
                );

                tiles.Add(
                    new ImageShape
                    {
                        ImageFill = new(tile, sourceShape.ImageFill.ImageScale),
                        BoundingBox = new BoundingBox(originX + left, originY + top, width, height),
                        Stroke = sourceShape.Stroke,
                        Opacity = sourceShape.Opacity,
                        Style = sourceShape.Style,
                    }
                );
            }
        }

        return tiles;
    }
}
