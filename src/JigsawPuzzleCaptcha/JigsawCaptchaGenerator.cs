using System.Security.Cryptography;
using JigsawPuzzleCaptcha.Contracts;
using JigsawPuzzleCaptcha.Core;
using JigsawPuzzleCaptcha.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace JigsawPuzzleCaptcha;

/// <summary>
/// Default <see cref="IJigsawCaptchaGenerator"/> implementation, backed by ImageSharp.
/// This type is stateless and thread-safe; register it as a singleton.
/// </summary>
public sealed class JigsawCaptchaGenerator : IJigsawCaptchaGenerator
{
    private readonly JigsawCaptchaOptions _defaults;

    /// <summary>Creates a generator using the default options.</summary>
    public JigsawCaptchaGenerator()
        : this(new JigsawCaptchaOptions())
    {
    }

    /// <summary>Creates a generator using the supplied options as its defaults.</summary>
    /// <param name="defaults">Options applied when a per-call override is not supplied.</param>
    /// <exception cref="ArgumentNullException"><paramref name="defaults"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">An option value is invalid.</exception>
    public JigsawCaptchaGenerator(JigsawCaptchaOptions defaults)
    {
        ArgumentNullException.ThrowIfNull(defaults);
        defaults.Validate();
        _defaults = defaults;
    }

    /// <inheritdoc />
    public CaptchaResult Generate(byte[] imageBytes, JigsawCaptchaOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(imageBytes);
        if (imageBytes.Length == 0)
        {
            throw new ArgumentException("The image data is empty.", nameof(imageBytes));
        }

        using Image<Rgba32> source = Image.Load<Rgba32>(imageBytes);
        return Create(source, Resolve(options));
    }

    /// <inheritdoc />
    public CaptchaResult Generate(Stream imageStream, JigsawCaptchaOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(imageStream);

        using Image<Rgba32> source = Image.Load<Rgba32>(imageStream);
        return Create(source, Resolve(options));
    }

    /// <inheritdoc />
    public bool Validate(int submittedX, int expectedX, int? tolerance = null)
    {
        int allowed = tolerance ?? _defaults.Tolerance;
        if (allowed < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), allowed, "Tolerance cannot be negative.");
        }

        return Math.Abs(submittedX - expectedX) <= allowed;
    }

    private JigsawCaptchaOptions Resolve(JigsawCaptchaOptions? options)
    {
        if (options is null)
        {
            return _defaults;
        }

        options.Validate();
        return options;
    }

    private static CaptchaResult Create(Image<Rgba32> source, JigsawCaptchaOptions options)
    {
        float tabRadius = MathF.Min(options.PieceWidth, options.PieceHeight) * options.TabRatio;
        int tabOffset = (int)MathF.Ceiling(tabRadius);

        // The canvas is taller than the piece body so the top tab is not clipped off.
        int canvasWidth = options.PieceWidth;
        int canvasHeight = options.PieceHeight + tabOffset;

        // The slot always sits in the right half, so the piece has room to slide.
        int minX = source.Width / 2;
        int maxX = source.Width - canvasWidth - options.Padding;
        int minY = options.Padding;
        int maxY = source.Height - canvasHeight - options.Padding;

        if (maxX < minX || maxY < minY)
        {
            int requiredWidth = (2 * (canvasWidth + options.Padding)) + 1;
            int requiredHeight = canvasHeight + (2 * options.Padding);
            throw new ArgumentException(
                $"The source image is {source.Width}x{source.Height}, which is too small for a " +
                $"{options.PieceWidth}x{options.PieceHeight} piece with padding {options.Padding}. " +
                $"It must be at least {requiredWidth}x{requiredHeight}.",
                nameof(source));
        }

        // The answer is a secret, so it comes from a cryptographic RNG rather than Random.Shared.
        int targetX = RandomNumberGenerator.GetInt32(minX, maxX + 1);
        int targetY = RandomNumberGenerator.GetInt32(minY, maxY + 1);

        IPath localPath = PuzzlePath.Create(options.PieceWidth, options.PieceHeight, tabRadius, tabOffset);
        IPath targetPath = localPath.Translate(targetX, targetY);

        using Image<Rgba32> piece = source.Clone(ctx =>
            ctx.Crop(new Rectangle(targetX, targetY, canvasWidth, canvasHeight)));

        using var mask = new Image<Rgba32>(canvasWidth, canvasHeight);
        mask.Mutate(ctx => ctx.Fill(Color.White, localPath));

        ApplyMask(piece, mask);

        piece.Mutate(ctx => ctx.Draw(Color.White.WithAlpha(0.8f), 1.5f, localPath));

        using Image<Rgba32> background = source.Clone();
        background.Mutate(ctx =>
        {
            ctx.Fill(Color.Black.WithAlpha(0.6f), targetPath);
            ctx.Draw(Color.White.WithAlpha(0.5f), 1.5f, targetPath);
        });

        var encoder = new PngEncoder { ColorType = PngColorType.RgbWithAlpha };

        using var backgroundStream = new MemoryStream();
        using var pieceStream = new MemoryStream();
        background.Save(backgroundStream, encoder);
        piece.Save(pieceStream, encoder);

        return new CaptchaResult(
            backgroundStream.ToArray(),
            pieceStream.ToArray(),
            targetX,
            targetY,
            canvasWidth,
            canvasHeight,
            source.Width,
            source.Height);
    }

    /// <summary>
    /// Clears every pixel outside the puzzle outline and feathers the anti-aliased edge,
    /// using row spans rather than the per-pixel indexer.
    /// </summary>
    private static void ApplyMask(Image<Rgba32> piece, Image<Rgba32> mask)
    {
        piece.ProcessPixelRows(mask, (pieceAccessor, maskAccessor) =>
        {
            for (int y = 0; y < pieceAccessor.Height; y++)
            {
                Span<Rgba32> pieceRow = pieceAccessor.GetRowSpan(y);
                Span<Rgba32> maskRow = maskAccessor.GetRowSpan(y);

                for (int x = 0; x < pieceRow.Length; x++)
                {
                    byte coverage = maskRow[x].A;
                    if (coverage == 0)
                    {
                        pieceRow[x] = default;
                    }
                    else if (coverage < byte.MaxValue)
                    {
                        pieceRow[x].A = (byte)(pieceRow[x].A * coverage / byte.MaxValue);
                    }
                }
            }
        });
    }
}
